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