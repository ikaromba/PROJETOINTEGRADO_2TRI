# -*- coding: utf-8 -*-
import os
from flask import Flask, request, jsonify
from flask_cors import CORS
from datetime import datetime
import joblib

app = Flask(__name__)
CORS(app)

# 1. Localiza a pasta onde o server.py está salvo e aponta para o modelo_ia.pkl
DIR_ATUAL = os.path.dirname(os.path.abspath(__file__))
CAMINHO_MODELO = os.path.join(DIR_ATUAL, 'modelo_ia.pkl')

# 2. Carrega o modelo de Inteligência Artificial
try:
    modelo_ia = joblib.load(CAMINHO_MODELO)
    print(f"[IA] Sucesso! Modelo 'modelo_ia.pkl' carregado de: {CAMINHO_MODELO}")
except Exception as e:
    print(f"[Avisos IA] Não foi possível carregar 'modelo_ia.pkl': {e}")
    modelo_ia = None


MAPA_CLASSES = {
    0: "Cheio",
    1: "Médio",
    2: "Baixo"
}

# Histórico de medições armazenadas em memória
historico_medicoes = []

def classificar_adc(valor_adc):
    """
    Função que passa o valor ADC para a IA (ou regra de fallback)
    e retorna o texto da classificação.
    """
    if modelo_ia is not None:
        try:
            # Lista Python padrão substituindo o numpy.array([[valor_adc]])
            entrada = [[float(valor_adc)]]
            predicao = int(modelo_ia.predict(entrada)[0])
            return MAPA_CLASSES.get(predicao, "Baixo")
        except Exception as err:
            print(f"[Erro Predição IA]: {err}")
    
    # Fallback básico caso o modelo_ia.pkl não esteja presente/carregado
    valor = float(valor_adc)
    if valor > 2700:
        return "Cheio"
    elif valor >= 1300:
        return "Médio"
    else:
        return "Baixo"

@app.route('/medicao', methods=['POST'])
def receber_medicao():
    """
    Rota chamada pelo C# ou pelo Simulador Web para registrar um novo dado
    """
    try:
        dados = request.get_json()
        valor_adc = dados.get('valor_adc')

        if valor_adc is None:
            return jsonify({'erro': 'Campo valor_adc não informado'}), 400

        classificacao = classificar_adc(valor_adc)
        horario_atual = datetime.now().strftime("%H:%M:%S")

        nova_medicao = {
            "valor_adc": int(valor_adc),
            "classificacao_ia": classificacao,
            "horario": horario_atual
        }

        # Insere a medição mais recente no início da lista
        historico_medicoes.insert(0, nova_medicao)

        # Mantém apenas os últimos 20 registros
        if len(historico_medicoes) > 20:
            historico_medicoes.pop()

        print(f"[NOVO DADO] ADC: {valor_adc} | IA: {classificacao} | Hora: {horario_atual}")
        return jsonify(nova_medicao), 200

    except Exception as e:
        return jsonify({'erro': str(e)}), 500

@app.route('/ultimas-medicoes', methods=['GET'])
def obter_ultimas_medicoes():
    """
    Rota chamada pelo JavaScript do site para atualizar visores e tabela
    """
    return jsonify(historico_medicoes), 200

if __name__ == '__main__':
    print("==================================================")
    print(" Servidor Flask / IA rodando em http://127.0.0.1:8000")
    print("==================================================")
    app.run(host='127.0.0.1', port=8000, debug=True)