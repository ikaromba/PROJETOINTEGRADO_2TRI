from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import joblib
import numpy as np
from datetime import datetime

#cria o servidor
app = FastAPI()

# libera o acesso 
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"], 
)

# IA
modelo_ia = joblib.load("modelo_ia.pkl")

historico_medicoes = []

class DadosMedicao(BaseModel):
    valor_adc: int

#  recebe a medição do C#, classificando na IA e salva no histórico
@app.post("/medicao")
def receber_medicao(dados: DadosMedicao):
   
    entrada = np.array([[dados.valor_adc]])
    
    # chama a IA para analisar
    resultado_num = int(modelo_ia.predict(entrada)[0])
    
    # traduz o numero para texto
    mapa_classes = {0: "Cheio", 1: "Médio", 2: "Baixo"}
    classificacao = mapa_classes.get(resultado_num, "Desconhecido")
    
    hora_atual = datetime.now().strftime("%H:%M:%S")
    
    nova_leitura = {
        "valor_adc": dados.valor_adc,
        "classificacao_ia": classificacao,
        "horario": hora_atual
    }
    
    historico_medicoes.insert(0, nova_leitura)
    
    # apenas as últimas 20 leituras na memória
    if len(historico_medicoes) > 20:
        historico_medicoes.pop()
        
    return {"status": "sucesso", "dados": nova_leitura}

# O site ve essa rota para atualizar a tela
@app.get("/ultimas-medicoes")
def obter_medicoes():
    return historico_medicoes