# PROJETOINTEGRADO_2TRI

# Sistema Integrado de Monitoramento de Variáveis Físicas (IoT + IA)

![Status](https://img.shields.io/badge/Status-Conclu%C3%ADdo-brightgreen)
![Instituição](https://img.shields.io/badge/ETE_FMC-3º_DS_--_Projeto_Integrado-blue)

> Solução distribuída estilo IoT para aquisição de dados via microcontrolador STM32, ponte de comunicação em C#, API REST com modelo de Inteligência Artificial em Python para classificação automatizada e Dashboard Web para monitoramento em tempo real.

---

## Demonstração em Vídeo

---VIDEO INDISPONIVEL----
[![Assista no YouTube](https://img.shields.io/badge/YouTube-Assistir_Apresentação-red?logo=youtube)](LINK_DO_SEU_VIDEO_NO_YOUTUBE)

---

## Integrantes do Grupo

* ** Íkaro ** - [@ikaromba](https://github.com/ikaromba)
* ** João Pedro ** - [@JPBillGit](https://github.com/JPBillGit)

---

## 📐 Arquitetura do Sistema

O fluxo de dados da solução ocorre de forma integrada entre os 5 módulos principais:

```text
[ Sensor / Trimpot ]
        │
        ▼
[ Firmware STM32 ] ──(USB CDC / Serial)──► [ Aplicação C# ]
                                                  │
                                            (HTTP / JSON)
                                                  │
                                                  ▼
[ Interface Web ] ◄────────(HTTP)───────── [ API REST + IA (Python) ]
