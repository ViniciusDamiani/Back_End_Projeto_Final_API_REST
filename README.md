# 🏠 SmartRoom – Automação de Quarto com API REST e IoT

Projeto desenvolvido na disciplina de **Back-End** do curso de **Engenharia de Computação** da **Unisatc**, com foco em **Internet das Coisas (IoT)**.  
O **SmartRoom** é uma aplicação que integra uma **API RESTful** com o **ESP32**, simulando a automação de um quarto moderno e inteligente.

---

## 🎯 Objetivo

Criar uma **API REST** capaz de controlar e monitorar dispositivos de um quarto, como luzes e ar-condicionado.  
A API envia comandos ao **ESP32**, que executa as ações fisicamente (ligar/desligar LEDs e ventoinha), simulando a automação residencial.

---

## 💡 Funcionalidades Principais

- **Controle de dispositivos:**
  - Luz do quarto → LED 1  
  - Luz da cabeceira → LED 2  
  - Ar-condicionado → Ventoinha  
- **Envio e recebimento de comandos via API REST**
- **Registro de status dos dispositivos no banco de dados**
- **Integração entre software e hardware**

---

## 🌐 Tecnologias Utilizadas

| Camada | Tecnologia |
|--------|-------------|
| **Back-End** | Node.js + Express |
| **Banco de Dados** | PostgreSQL / SQLite |
| **Microcontrolador** | ESP32 |
| **Linguagem do Firmware** | C++ (Arduino IDE) |
| **Documentação de Rotas** | Swagger / Postman |
| **Versionamento** | Git + GitHub |

---

## 🔌 Estrutura Física

| Componente | Função |
|-------------|--------|
| ESP32 | Controlador principal |
| LED 1 | Luz do quarto |
| LED 2 | Luz da cabeceira |
| Ventoinha | Simulação do ar-condicionado |
| Resistores + Protoboard | Montagem e proteção de circuito |

O **ESP32** se comunica com a API enviando e recebendo dados via HTTP, executando as ações de controle físico.

---

## 👨‍💻 Integrantes

- **Victor Bonomi**  
- **Rafael Webber**  
- **Vinicius Damiani**

---

## 📚 Instituição

> **UNISATC** – Engenharia de Computação  
> Disciplina: **Back-End**  

---

## 📸 Observação

As fotos e esquemas de montagem do circuito serão adicionados conforme o avanço das etapas do projeto.

---
