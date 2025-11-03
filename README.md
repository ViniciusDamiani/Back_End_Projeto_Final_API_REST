# 🌿 SmartBonsai – Vaso Inteligente

Projeto desenvolvido na disciplina de **Back-End** do curso de **Engenharia de Computação** da **Unisatc**, com foco em **Internet das Coisas (IoT)**.  
O **SmartBonsai** é um sistema inteligente de monitoramento e controle de um vaso de bonsai, integrando sensores a uma **API RESTful** desenvolvida em **C#**.

---

## 🎯 Objetivo

O projeto tem como objetivo desenvolver um **vaso de bonsai automatizado**, capaz de **monitorar e controlar as condições ideais de cultivo**.  
Por meio de sensores e dispositivos conectados a um **ESP32**, o sistema realiza medições de **temperatura, umidade e luminosidade**, acionando automaticamente **irrigação, ventilação e iluminação** conforme a necessidade.

Além do modo automático, o sistema permite o **controle remoto via API REST**, possibilitando comandos diretos para ligar ou desligar cada dispositivo.

---

## 🌱 Funcionalidades Principais

- 🌡️ **Monitoramento de temperatura e umidade** do ambiente do vaso.  
- 💧 **Controle automático de irrigação** através de um motor peristáltico e sensor de umidade do solo.  
- 🌬️ **Sistema de ventilação (cooler)** para secagem em caso de excesso de umidade.  
- 💡 **Controle de iluminação** com LEDs para simular luz solar em ambientes internos.  
- 🧠 **Decisões automáticas** baseadas em limites configuráveis.  
- 📡 **Integração com API REST (C# + ESP32)** para controle manual e leitura remota dos dados.  
- 🖥️ **Display LCD** exibindo em tempo real as medições e status do sistema.  

---

## 👾 Cartas-Desafio
 - **Notificações via Email** - Implemente uma funcionalidade que envie notificações por email para os usuários quando determinadas ações ocorrerem na API, como a criação ou atualização de um registro. A rota deve permitir configurar o envio de notificações e gerenciar as preferências dos usuários em relação às notificações recebidas;

---

## 🔌 Componentes Utilizados

| Componente | Função |
|-------------|--------|
| **ESP32** | Microcontrolador principal e comunicação com a API |
| **Sensor DHT22** | Leitura de temperatura e umidade do ar |
| **Sensor de umidade do solo** | Verifica a necessidade de irrigação |
| **Motor peristáltico** | Sistema de irrigação automática |
| **Cooler (Air Cooler)** | Secagem do solo em caso de umidade alta |
| **LDR (Sensor de luz)** | Mede a luminosidade ambiente |
| **Relés (até 6)** | Acionamento dos dispositivos (bomba, LEDs, cooler, etc.) |
| **LEDs** | Simulação de iluminação artificial |
| **Display LCD 16x2** | Exibição de informações do sistema |

---

## 📡 Rotas da API – SmartBonsai

### 🌿 Measurements Controller

| **Método / Rota** | **Descrição** | **Validação** | **Respostas** |
|--------------------|----------------|----------------|----------------|
| **GET** `/api/devices/{deviceId}/measurements/latest` | Retorna a última medição registrada para o dispositivo informado. | `Guid.Empty → 400` | `200 OK` com os dados da última medição / `404 NotFound` se não houver medição registrada |
| **POST** `/api/devices/{deviceId}/measurements` | Registra uma nova medição enviada pelo dispositivo. | `deviceId` válido | `201 Created` com os dados da medição criada |

---

### ⚙️ Actuators Controller

| **Método / Rota** | **Descrição / Objetivo** | **Validação** | **Respostas** |
|--------------------|--------------------------|----------------|----------------|
| **POST** `/api/actuators/{id}/commands` | Envia um comando ao atuador (ex: ligar bomba). | - | `200 OK` com mensagem de sucesso / `404 NotFound` se o atuador não for encontrado |
| **GET** `/api/actuators/{id}/status` | Retorna o status atual do atuador (ligado/desligado, intensidade etc.). | - | `200 OK` com o status atual / `404 NotFound` se o atuador não existir |

---

### 🤖 Automation Controller

| **Método / Rota** | **Descrição / Objetivo** | **Validação** | **Respostas** |
|--------------------|--------------------------|----------------|----------------|
| **GET** `/api/automation/status` | Retorna o estado atual da automação (ativa ou desativada). | - | `200 OK` |
| **POST** `/api/automation/enable` | Ativa o modo automático do sistema. | - | `200 OK` |
| **POST** `/api/automation/disable` | Desativa o modo automático. | - | `200 OK` |
| **PUT** `/api/automation/targets` | Atualiza as metas e limites da automação (`AutomationStatusDto`). | `ModelState.IsValid` | `200 OK` / `400 Bad Request` |
| **POST** `/api/automation/evaluate` | Executa a lógica de automação com base nas metas configuradas. | - | `200 OK` |

---

## 🌐 Tecnologias Utilizadas

| Camada | Tecnologia |
|--------|-------------|
| **Back-End** | C# + ASP.NET Core |
| **Microcontrolador** | ESP32 |
| **Linguagem de Firmware** | C++ (Arduino IDE) |
| **Documentação de Rotas** | Swagger / Postman |
| **Versionamento** | Git + GitHub |

---

## 🖨️ Impressão 3D

Com a **impressora 3D**, será desenvolvido um **suporte personalizado para o vaso**, contendo:
- Alojamento para o ESP32 e relés;  
- Dutos de ventilação para o cooler;  
- Compartimento para LEDs e sensores;  
- Base para o motor peristáltico e mangueiras de irrigação.  

A estrutura será projetada para **simular um vaso de bonsai real**, integrando estética e funcionalidade.

---

## 📚 Instituição

> **UNISATC** – Engenharia de Computação  
> Disciplina: **Back-End (Projeto Final – API REST)**  

---

## 👨‍💻 Integrantes

- **Victor Bonomi**  
- **Rafael Webber**  
- **Vinicius Damiani**

---

## 📸 Observação

As imagens, vídeos e diagramas da montagem física do sistema serão adicionados conforme o avanço das etapas do projeto.

---
