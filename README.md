# PROJETOINTEGRADO_2TRI

# Sistema Integrado de Monitoramento de Variáveis Físicas (IoT + IA)

![Status](https://img.shields.io/badge/Status-Andamento-brightgreen)
![Instituição](https://img.shields.io/badge/ETE_FMC-3º_DS_--_Projeto_Integrado-blue)
![STM32](https://img.shields.io/badge/STM32-03234C?style=for-the-badge&logo=stmicroelectronics&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Python](https://img.shields.io/badge/Python-3776AB?style=for-the-badge&logo=python&logoColor=white)
![HTML5](https://img.shields.io/badge/HTML5-E34F26?style=for-the-badge&logo=html5&logoColor=white)

> Solução distribuída estilo IoT para aquisição de dados via microcontrolador STM32, ponte de comunicação em C#, API REST com modelo de Inteligência Artificial em Python para classificação automatizada e Dashboard Web para monitoramento em tempo real.

---

##  Integrantes do Grupo

* **Íkaro** - [@ikaromba](https://github.com/ikaromba)
* **João Pedro** - [@JPBillGit](https://github.com/JPBillGit)

---

## Demonstração em Vídeo

[![YouTube](https://img.shields.io/badge/YouTube-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://youtu.be/ejJNGc2txm4)

**[Clique aqui para assistir à apresentação no YouTube](https://youtu.be/ejJNGc2txm4)**

---

##  Arquitetura do Sistema

O fluxo de dados da solução ocorre de forma integrada entre os módulos principais do sistema:

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
