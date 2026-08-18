
import os
from flask import Flask, request, jsonify
from flask_cors import CORS
from datetime import datetime
import joblib

app = Flask(__name__)
CORS(app)

DIR_ATUAL = os.path.dirname(os.path.abspath(__file__))
CAMINHO_MODELO = os.path.join(DIR_ATUAL, 'modelo_ia.pkl')

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

# Inicializa com uma medição padrão para a tela não ficar zerada
historico_medicoes = [
    {
        "valor_adc": 2000,
        "classificacao_ia": "Médio",
        "horario": datetime.now().strftime("%H:%M:%S")
    }
]

def classificar_adc(valor_adc):
    try:
        valor = float(valor_adc)
    except (ValueError, TypeError):
        valor = 0.0

    if modelo_ia is not None:
        try:
            entrada = [[valor]]
            predicao = int(modelo_ia.predict(entrada)[0])
            return MAPA_CLASSES.get(predicao, "Baixo")
        except Exception as err:
            print(f"[Erro Predição IA]: {err}")

    # Fallback de segurança
    if valor > 2700:
        return "Cheio"
    elif valor >= 1300:
        return "Médio"
    else:
        return "Baixo"

@app.route('/medicao', methods=['POST'])
def receber_medicao():
    try:
        dados = request.get_json(silent=True) or {}
        valor_adc = dados.get('valor_adc')

        if valor_adc is None:
            return jsonify({'erro': 'Campo valor_adc não informado'}), 400

        try:
            valor_adc_int = int(float(valor_adc))
        except (ValueError, TypeError):
            return jsonify({'erro': 'Valor ADC inválido'}), 400

        classificacao = classificar_adc(valor_adc_int)
        horario_atual = datetime.now().strftime("%H:%M:%S")

        nova_medicao = {
            "valor_adc": valor_adc_int,
            "classificacao_ia": classificacao,
            "horario": horario_atual
        }

        historico_medicoes.insert(0, nova_medicao)

        if len(historico_medicoes) > 20:
            historico_medicoes.pop()

        print(f"[NOVO DADO] ADC: {valor_adc_int} | IA: {classificacao} | Hora: {horario_atual}")
        return jsonify(nova_medicao), 200

    except Exception as e:
        print(f"[Erro no Servidor]: {e}")
        return jsonify({'erro': str(e)}), 500

@app.route('/ultimas-medicoes', methods=['GET'])
def obter_ultimas_medicoes():
    return jsonify(historico_medicoes), 200

if __name__ == '__main__':
    print("==================================================")
    print(" Servidor Flask / IA rodando em http://127.0.0.1:8000")
    print("==================================================")
    app.run(host='127.0.0.1', port=8000, debug=True)