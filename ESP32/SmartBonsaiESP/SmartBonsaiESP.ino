#include <WiFi.h>
#include <HTTPClient.h>
#include <ArduinoJson.h>
#include <Adafruit_Sensor.h>
#include <DHT.h>

const char* ssid = "-Ademir 2GHz";
const char* password = "996730031";
const char* server = "http://192.168.0.119:5000"; // IP da API

#define PINSENSORLUMINOSIDADE 32
#define PINSENSORDHT11 33
#define PINSENSORUMIDADESOLO 35
#define PINSENSORFLUXOAGUA 2
#define PIN_LED_UV 13
#define PIN_BUZZER 15
#define LED_UV_ACTIVE_LOW 1
#define BUZZER_ACTIVE_HIGH 0

#define PIN_BOMBAREFRESH 4
#define PIN_BOMBA 5

#define FLOW_DEVICE_ID 4
const char* ACTUATOR_WATER = "00000000-0000-0000-0000-000000000001";
const char* ACTUATOR_UV = "00000000-0000-0000-0000-000000000002";
const char* ACTUATOR_BUZZER = "00000000-0000-0000-0000-000000000004";
const char* ACTUATOR_AERATOR = "00000000-0000-0000-0000-000000000003"; // usa id do FAN

#define DHTTYPE DHT11

// Fluxo: ajuste conforme o sensor (YF-S201/YF-S401 tipicamente 7.5 Hz por L/min)
#define FLOW_PIN PINSENSORFLUXOAGUA
volatile unsigned long pulseCount = 0;
unsigned long lastPulseCount = 0;
unsigned long lastFlowCalcMillis = 0;
float flowRateLpm = 0.0f;
float totalLiters = 0.0f;
const float calibrationFactor = 7.5f;
unsigned long irrigacaoInicio = 0;
bool bombaLigada = false;
bool buzzerShouldBeOn = false;
bool buzzerState = false;
unsigned long buzzerStepStarted = 0;
int buzzerPatternStep = 0;

DHT dht(PINSENSORDHT11, DHTTYPE);

void IRAM_ATTR pulseCounter() {
  pulseCount++;
}

void setup() {
  Serial.begin(115200);
  WiFi.begin(ssid, password);
  Serial.println("==========================================");
  Serial.println("SmartBonsai - Vaso Inteligente");
  Serial.println("==========================================");

  Serial.println("Conectando ao WiFi ");
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("\nWiFi conectado!");
  Serial.print("Endereco IP local: ");
  Serial.println(WiFi.localIP());

  pinMode(PIN_BOMBA, OUTPUT);
  digitalWrite(PIN_BOMBA, LOW);

  pinMode(PIN_BOMBAREFRESH, OUTPUT);
  digitalWrite(PIN_BOMBAREFRESH, LOW);

  pinMode(PIN_LED_UV, OUTPUT);
  digitalWrite(PIN_LED_UV, LED_UV_ACTIVE_LOW ? HIGH : LOW); // garante desligado no boot

  pinMode(PIN_BUZZER, OUTPUT);
  digitalWrite(PIN_BUZZER, LOW);

  pinMode(FLOW_PIN, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(FLOW_PIN), pulseCounter, RISING);

  dht.begin();
  Serial.println("Sensor DHT11 inicializado com sucesso!");
}

void loop() {
  if (WiFi.status() == WL_CONNECTED) {
    priSensorUmidadeSolo();
    priSensorTemperatura();
    priSensorLuminosidade();
    priComandoBomba();
    priComandoUv();
    priComandoAerador();
    priComandoBuzzer();
    atualizarFluxoDuranteIrrigacao();
    atualizarBuzzerPattern();
  } else {
    Serial.println("WiFi desconectado! Tentando reconectar...");
    WiFi.reconnect();
  }
  delay(500); // janela menor para atualizar fluxo/buzzer e sensores
}

void priSensorUmidadeSolo() {
  Serial.println("");
  Serial.println("==========================================");
  Serial.println("Iniciando Leitura da Umidade do Solo: ");
  Serial.println("==========================================");

  int iSensorUmidadeSolo = analogRead(PINSENSORUMIDADESOLO);
  float fSensorUmidadeSolo = map(iSensorUmidadeSolo, 4095, 0, 0, 100);
  Serial.print("Umidade do solo: ");
  Serial.print(fSensorUmidadeSolo);
  Serial.println("%");

  StaticJsonDocument<200> doc;
  doc["SoilHumidityPct"] = fSensorUmidadeSolo;
  doc["AirHumidityPct"] = 0;
  doc["TemperatureC"] = 0;

  String payload;
  serializeJson(doc, payload);
  Serial.println("Payload JSON: " + payload);

  HTTPClient http;
  http.begin(String(server) + "/api/devices/1/measurements");
  http.addHeader("Content-Type", "application/json");
  int httpResponseCode = http.POST(payload);

  if (httpResponseCode > 0) {
    Serial.println("POST enviado! Codigo HTTP: " + String(httpResponseCode));
    Serial.println("ID Dispositivo: 1");
  } else {
    Serial.println("Falha no envio POST. Erro: " + String(httpResponseCode));
  }

  Serial.println("==========================================");
  Serial.println("Finalizando Leitura da Umidade do Solo");
  Serial.println("==========================================");
  Serial.println("");

  http.end();
}

void priSensorTemperatura() {
  Serial.println("");
  Serial.println("==========================================");
  Serial.println("Iniciando Leitura do Sensor DHT11: ");
  Serial.println("==========================================");

  delay(2000); // necessario para o DHT11 funcionar no ESP32

  float fUmidadeAr = dht.readHumidity();
  float fTemperatura = dht.readTemperature();

  if (isnan(fUmidadeAr) || isnan(fTemperatura)) {
    Serial.println("Erro ao ler o DHT11!");
    return;
  }

  Serial.print("Temperatura: ");
  Serial.print(fTemperatura);
  Serial.println(" C");

  Serial.print("Umidade do ar: ");
  Serial.print(fUmidadeAr);
  Serial.println(" %");

  StaticJsonDocument<200> doc;
  doc["SoilHumidityPct"] = 0;
  doc["AirHumidityPct"] = fUmidadeAr;
  doc["TemperatureC"] = fTemperatura;

  String payload;
  serializeJson(doc, payload);
  Serial.println("Payload JSON: " + payload);

  HTTPClient http;
  http.begin(String(server) + "/api/devices/2/measurements");
  http.addHeader("Content-Type", "application/json");
  int httpResponseCode = http.POST(payload);

  if (httpResponseCode > 0) {
    Serial.println("POST enviado! Codigo HTTP: " + String(httpResponseCode));
    Serial.println("ID Dispositivo: 2");
  } else {
    Serial.println("Falha no envio POST. Erro: " + String(httpResponseCode));
  }

  Serial.println("==========================================");
  Serial.println("Finalizando Leitura do Sensor DHT11: ");
  Serial.println("==========================================");
  Serial.println("");

  http.end();
}

void priSensorLuminosidade() {
  Serial.println("");
  Serial.println("==========================================");
  Serial.println("Iniciando Leitura do Sensor de Luminosidade (Digital D0): ");
  Serial.println("==========================================");

  int estadoLuz = digitalRead(PINSENSORLUMINOSIDADE);
  float fSensorLuminosidade = (estadoLuz == HIGH) ? 100.0f : 0.0f;

  if (estadoLuz == HIGH) {
    Serial.println("Ambiente claro");
  } else {
    Serial.println("Ambiente escuro");
  }

  Serial.print("Luminosidade: ");
  Serial.print(fSensorLuminosidade);
  Serial.println("%");

  StaticJsonDocument<200> doc;
  doc["SoilHumidityPct"] = 0;
  doc["AirHumidityPct"] = 0;
  doc["TemperatureC"] = 0;
  doc["LightPct"] = fSensorLuminosidade;

  String payload;
  serializeJson(doc, payload);
  Serial.println("Payload JSON: " + payload);

  HTTPClient http;
  http.begin(String(server) + "/api/devices/3/measurements");
  http.addHeader("Content-Type", "application/json");
  int httpResponseCode = http.POST(payload);

  if (httpResponseCode > 0) {
    Serial.println("POST enviado! Codigo HTTP: " + String(httpResponseCode));
    Serial.println("ID Dispositivo: 3");
  } else {
    Serial.println("Falha no envio POST. Erro: " + String(httpResponseCode));
  }

  Serial.println("==========================================");
  Serial.println("Finalizando Leitura do Sensor de Luminosidade: ");
  Serial.println("==========================================");
  Serial.println("");

  http.end();
}

void priComandoBomba() {
  HTTPClient http;
  http.begin(String(server) + "/api/actuators/" + ACTUATOR_WATER + "/status");
  int code = http.GET();
  if (code == 200) {
    StaticJsonDocument<256> doc;
    if (deserializeJson(doc, http.getString()) == DeserializationError::Ok) {
      bool isActive = doc["isActive"] | false;
      bool wasActive = bombaLigada;

      digitalWrite(PIN_BOMBA, isActive ? HIGH : LOW);

      if (isActive && !wasActive) {
        pulseCount = 0;
        lastPulseCount = 0;
        totalLiters = 0.0f;
        flowRateLpm = 0.0f;
        irrigacaoInicio = millis();
        lastFlowCalcMillis = irrigacaoInicio;
      } else if (!isActive && wasActive) {
        enviarFluxoAposIrrigacao(millis());
        irrigacaoInicio = 0;
      }

      bombaLigada = isActive;
      Serial.println(isActive ? "Bomba ON" : "Bomba OFF");
    }
  } else {
    Serial.printf("Falha ao ler status da bomba: %d\n", code);
  }
  http.end();
}

void priComandoUv() {
  HTTPClient http;
  http.begin(String(server) + "/api/actuators/" + ACTUATOR_UV + "/status");
  int code = http.GET();
  if (code == 200) {
    StaticJsonDocument<256> doc;
    if (deserializeJson(doc, http.getString()) == DeserializationError::Ok) {
      bool isActive = doc["isActive"] | false;
      // Se o LED for ativo em nível baixo, inverte o nível lógico
      digitalWrite(PIN_LED_UV, isActive ? (LED_UV_ACTIVE_LOW ? LOW : HIGH)
                                        : (LED_UV_ACTIVE_LOW ? HIGH : LOW));
      Serial.println(isActive ? "UV LED ON" : "UV LED OFF");
    }
  } else {
    Serial.printf("Falha ao ler status da luz UV: %d\n", code);
  }
  http.end();
}

void priComandoAerador() {
  HTTPClient http;
  http.begin(String(server) + "/api/actuators/" + ACTUATOR_AERATOR + "/status");
  int code = http.GET();
  if (code == 200) {
    StaticJsonDocument<256> doc;
    if (deserializeJson(doc, http.getString()) == DeserializationError::Ok) {
      bool isActive = doc["isActive"] | false;
      digitalWrite(PIN_BOMBAREFRESH, isActive ? HIGH : LOW);
      Serial.println(isActive ? "Aerador ON" : "Aerador OFF");
    }
  } else {
    Serial.printf("Falha ao ler status do aerador: %d\n", code);
  }
  http.end();
}

void priComandoBuzzer() {
  HTTPClient http;
  http.begin(String(server) + "/api/actuators/" + ACTUATOR_BUZZER + "/status");
  int code = http.GET();
  if (code == 200) {
    StaticJsonDocument<256> doc;
    if (deserializeJson(doc, http.getString()) == DeserializationError::Ok) {
      bool isActive = doc["isActive"] | false;
      buzzerShouldBeOn = isActive;
      if (!isActive) {
        buzzerState = false;
        buzzerStepStarted = 0;
        buzzerPatternStep = 0;
        digitalWrite(PIN_BUZZER, BUZZER_ACTIVE_HIGH ? LOW : HIGH);
      } else {
        Serial.println("Buzzer ON (alerta)");
      }
    }
  } else {
    Serial.printf("Falha ao ler status do buzzer: %d\n", code);
  }
  http.end();
}

// Padrão de beep: dois toques curtos e uma pausa (ex.: 120ms on, 120ms off, 120ms on, 600ms off)
void atualizarBuzzerPattern() {
  if (!buzzerShouldBeOn) return;
  unsigned long now = millis();
  // Padrão customizado: longo-curto-longo e pausa (200ms on, 120ms off, 150ms on, 400ms off)
  const int patternDurations[] = {200, 120, 150, 400};
  const bool patternStates[] = {true, false, true, false};
  const int patternLen = 4;

  if (buzzerStepStarted == 0) {
    buzzerStepStarted = now;
    buzzerPatternStep = 0;
    buzzerState = patternStates[0];
    digitalWrite(PIN_BUZZER, buzzerState ? (BUZZER_ACTIVE_HIGH ? HIGH : LOW)
                                         : (BUZZER_ACTIVE_HIGH ? LOW : HIGH));
    return;
  }

  int currentDuration = patternDurations[buzzerPatternStep];
  if (now - buzzerStepStarted >= (unsigned long)currentDuration) {
    buzzerPatternStep = (buzzerPatternStep + 1) % patternLen;
    buzzerStepStarted = now;
    buzzerState = patternStates[buzzerPatternStep];
    digitalWrite(PIN_BUZZER, buzzerState ? (BUZZER_ACTIVE_HIGH ? HIGH : LOW)
                                         : (BUZZER_ACTIVE_HIGH ? LOW : HIGH));
  }
}

void atualizarFluxoDuranteIrrigacao() {
  if (!bombaLigada) return;
  unsigned long now = millis();
  if (now - lastFlowCalcMillis < 1000) return;

  unsigned long deltaMillis = now - lastFlowCalcMillis;
  unsigned long pulsesThisWindow = pulseCount - lastPulseCount;
  lastPulseCount = pulseCount;
  lastFlowCalcMillis = now;

  float deltaSec = deltaMillis / 1000.0f;
  if (deltaSec <= 0) return;

  flowRateLpm = (pulsesThisWindow / calibrationFactor) / deltaSec; // L/min
  totalLiters += flowRateLpm * (deltaSec / 60.0f);

  Serial.print("Pulsos janela: ");
  Serial.print(pulsesThisWindow);
  Serial.print(" | Vazao inst: ");
  Serial.print(flowRateLpm, 3);
  Serial.print(" L/min | Total ciclo: ");
  Serial.print(totalLiters, 3);
  Serial.println(" L");
}

void enviarFluxoAposIrrigacao(unsigned long fimIrrigacaoMillis) {
  if (irrigacaoInicio == 0) return;

  unsigned long elapsed = fimIrrigacaoMillis - irrigacaoInicio;
  float litrosCiclo = totalLiters;
  // fallback: se nada foi integrado (totalLiters ~0), tentar converter pulsos acumulados
  if (litrosCiclo <= 0.0001f && pulseCount > 0) {
    litrosCiclo = pulseCount / (calibrationFactor * 60.0f); // equivalente a ~450 pulsos/L
  }
  float volumeMl = litrosCiclo * 1000.0f;
  float flowLpmMedio = (elapsed > 0) ? (litrosCiclo * (60000.0f / elapsed)) : 0.0f;

  Serial.printf("Encerrando irrigacao. Pulsos total: %lu | Litros: %.3f | mL: %.1f | Vazao media: %.3f L/min | Duracao ms: %lu\n",
                pulseCount, litrosCiclo, volumeMl, flowLpmMedio, elapsed);

  StaticJsonDocument<200> doc;
  doc["LightPct"] = 0;
  doc["SoilHumidityPct"] = 0;
  doc["AirHumidityPct"] = 0;
  doc["TemperatureC"] = 0;
  doc["WaterFlowLpm"] = flowLpmMedio;
  doc["WaterVolumeMl"] = volumeMl;

  String payload;
  serializeJson(doc, payload);
  Serial.println("Fluxo/volume do ciclo: " + payload);

  HTTPClient http;
  http.begin(String(server) + "/api/devices/" + String(FLOW_DEVICE_ID) + "/measurements");
  http.addHeader("Content-Type", "application/json");
  int httpResponseCode = http.POST(payload);

  if (httpResponseCode > 0) {
    Serial.printf("Fluxo enviado (ciclo completo): %.2f L/min, %.1f mL (HTTP %d)\n", flowLpmMedio, volumeMl, httpResponseCode);
  } else {
    Serial.printf("Falha ao enviar fluxo. Erro: %d\n", httpResponseCode);
  }

  http.end();
}
