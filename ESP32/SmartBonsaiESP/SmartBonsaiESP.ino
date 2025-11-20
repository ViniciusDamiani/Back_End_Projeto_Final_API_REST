#include <WiFi.h>
#include <HTTPClient.h>
#include <ArduinoJson.h>

#include <Adafruit_Sensor.h>
#include <DHT.h>
#include <DHT_U.h>

const char* ssid = "-Ademir 2GHz";
const char* password = "996730031";
const char* server = "http://192.168.0.113:5000"; // IP da API

#define PINSENSORUMIDADESOLO 34  // D34 - entrada analógica
#define PINSENSORDHT11 4
#define PINSENSORLUMINOSIDADE 35  // D35 - entrada analógica

#define PIN_BOMBA 5  // ajuste para seu relé
const char* ACTUATOR_WATER = "00000000-0000-0000-0000-000000000001";

#define DHTTYPE DHT11

DHT dht(PINSENSORDHT11, DHTTYPE); // inicializa o DHT11

void setup() {
  Serial.begin(115200);
  WiFi.begin(ssid, password);
  Serial.println("==========================================");
  Serial.println("SmartBonsai – Vaso Inteligente");
  Serial.println("==========================================");

  Serial.println("Conectando ao WiFi ");

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("\nWiFi conectado!");
  Serial.print("Endereço IP local: ");
  Serial.println(WiFi.localIP());

  pinMode(PIN_BOMBA, OUTPUT);
  digitalWrite(PIN_BOMBA, LOW);

  // Inicializa o sensor DHT11
  dht.begin();
  Serial.println("Sensor DHT11 inicializado com sucesso!");
}

void loop() {
  if (WiFi.status() == WL_CONNECTED) {

    //Leitura a cada 10s da umidade do solo
    priSensorUmidadeSolo();

    //Leitura a cada 10s da temperatura e umidade do ar
    priSensorTemperatura();

    //Leitura a cada 10s da luminosidade
    priSensorLuminosidade();

    //Leitura a cada 5s do comando na Bomba
    priComandoBomba();

  } else {
    Serial.println("WiFi desconectado! Tentando reconectar...");
    WiFi.reconnect();
  }
  delay(10000); //lê a cada 10 segundos
}

void priSensorUmidadeSolo() {
  Serial.println("");
  Serial.println("==========================================");
  Serial.println("Iniciando Leitura da Umidade do Solo: ");
  Serial.println("==========================================");

  //Lê o valor analógico do sensor
  int iSensorUmidadeSolo = analogRead(PINSENSORUMIDADESOLO);

  //Converte para porcentagem (0 = molhado, 100 = seco)
  float fSensorUmidadeSolo = map(iSensorUmidadeSolo, 4095, 0, 0, 100);
  Serial.print("Umidade do solo: ");
  Serial.print(fSensorUmidadeSolo);
  Serial.println("%");

  //Cria o JSON
  StaticJsonDocument<200> doc;
  doc["SoilHumidityPct"] = fSensorUmidadeSolo;
  doc["AirHumidityPct"] = 0;
  doc["TemperatureC"] = 0;

  String payload;
  serializeJson(doc, payload);
  Serial.println("Payload JSON: " + payload);

  // Envia o POST para a API
  HTTPClient http;
  http.begin(String(server) + "/api/devices/1/measurements");
  http.addHeader("Content-Type", "application/json");
  int httpResponseCode = http.POST(payload);

  if (httpResponseCode > 0) {
    Serial.println("POST enviado! Código HTTP: " + String(httpResponseCode));
    Serial.println("ID Dispositivo: 1");
  } else {
    Serial.println("Falha no envio POST. Erro: " + String(httpResponseCode));
  }

  Serial.println("==========================================");
  Serial.println("Finalizando Leitura da Umidade do Solo: ");
  Serial.println("==========================================");
  Serial.println("");

  http.end();
}

void priSensorTemperatura() {
  Serial.println("");
  Serial.println("==========================================");
  Serial.println("Iniciando Leitura do Sensor DHT11: ");
  Serial.println("==========================================");

  //Lê temperatura e umidade
  float fUmidadeAr = dht.readHumidity();
  float fTemperatura = dht.readTemperature();

  //Verifica se a leitura falhou
  if (isnan(fUmidadeAr) || isnan(fTemperatura)) {
    Serial.println("Erro ao ler o DHT11!");
    return;
  }

  Serial.print("Temperatura: ");
  Serial.print(fTemperatura);
  Serial.println(" °C");

  Serial.print("Umidade do ar: ");
  Serial.print(fUmidadeAr);
  Serial.println(" %");

  //Cria o JSON
  StaticJsonDocument<200> doc;

  doc["SoilHumidityPct"] = 0;
  doc["AirHumidityPct"] = fUmidadeAr;
  doc["TemperatureC"] = fTemperatura;

  String payload;
  serializeJson(doc, payload);
  Serial.println("Payload JSON: " + payload);

  //Envia o POST para a API
  HTTPClient http;
  http.begin(String(server) + "/api/devices/2/measurements");
  http.addHeader("Content-Type", "application/json");
  int httpResponseCode = http.POST(payload);

  if (httpResponseCode > 0) {
    Serial.println("POST enviado! Código HTTP: " + String(httpResponseCode));
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

  // Lê o valor digital do sensor (0 = escuro, 1 = claro)
  int estadoLuz = digitalRead(PINSENSORLUMINOSIDADE);

  // Converte para uma "porcentagem simbólica"
  float fSensorLuminosidade = (estadoLuz == HIGH) ? 100.0 : 0.0;

  // Mostra o valor lido no Serial Monitor
  if (estadoLuz == HIGH) {
    Serial.println("Ambiente claro");
  } else {
    Serial.println("Ambiente escuro");
  }

  Serial.print("Luminosidade: ");
  Serial.print(fSensorLuminosidade);
  Serial.println("%");

  // Cria o JSON para enviar à API
  StaticJsonDocument<200> doc;
  doc["SoilHumidityPct"] = 0;
  doc["AirHumidityPct"] = 0;
  doc["TemperatureC"] = 0;
  doc["LightPct"] = fSensorLuminosidade;  // 0 ou 100 conforme o estado

  String payload;
  serializeJson(doc, payload);
  Serial.println("Payload JSON: " + payload);

  // Envia o POST para a API
  HTTPClient http;
  http.begin(String(server) + "/api/devices/3/measurements");  // Device 3 = sensor de luz
  http.addHeader("Content-Type", "application/json");
  int httpResponseCode = http.POST(payload);

  if (httpResponseCode > 0) {
    Serial.println("POST enviado! Código HTTP: " + String(httpResponseCode));
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
      digitalWrite(PIN_BOMBA, isActive ? HIGH : LOW);
      Serial.println(isActive ? "Bomba ON" : "Bomba OFF");
    }
  } else {
    Serial.printf("Falha ao ler status da bomba: %d\n", code);
  }
  http.end();
}


