# -*- coding: utf-8 -*-
from flask import Flask, request, jsonify
from flask_cors import CORS
import joblib
import numpy as np

app = Flask(__name__)
CORS(app)

# 1. Carrega o modelo gerado pelo seu código no Colab
try:
    modelo_ia = joblib.load('modelo_ia.pkl')
    print("Modelo IA (modelo_ia.pkl) carregado com sucesso!")
except Exception as e:
    print(f"Erro ao carregar 'modelo_ia.pkl': {e}")
    modelo_ia = None

# Dicionário para mapear as classes (0, 1, 2) da sua IA para texto
MAPA_CLASSES = {
    0: "Tanque Cheio",
    1: "Nível Médio",
    2: "Reserva / Nível Baixo"
}

# Armazena a última leitura processada pela IA
ultimo_dado = {
    "media_recebida": 0,
    "litros": 0.0,
    "classe": 2,
    "status": "Aguardando dados..."
}

# Rota POST: O C# envia a media_adc da STM32 aqui
@app.route('/predict', methods=['POST'])
def predict():
    global ultimo_dado
    try:
        dados = request.get_json()
        valor_medio = dados.get('media_adc')
        
        if valor_medio is None:
            return jsonify({'erro': 'Campo media_adc nao fornecido'}), 400

        valor_float = float(valor_medio)

        # Regra de três para calcular os Litros (4095 ADC = 40 Litros)
        litros = round((valor_float * 40.0) / 4095.0, 1)

        # 2. Usa a IA (DecisionTree) para classificar o dado
        if modelo_ia is not None:
            # O modelo espera uma matriz 2D: [[ valor ]]
            entrada = np.array([[valor_float]])
            predicao_classe = int(modelo_ia.predict(entrada)[0])
            status_texto = MAPA_CLASSES.get(predicao_classe, "Desconhecido")
        else:
            predicao_classe = -1
            status_texto = "Modelo nao carregado"

        # Atualiza a memória do servidor
        ultimo_dado = {
            'media_recebida': round(valor_float, 2),
            'litros': litros,
            'classe': predicao_classe,
            'status': status_texto
        }

        print(f"IA Processou -> ADC: {valor_float:.2f} | Litros: {litros}L | Status: {status_texto}")
        return jsonify(ultimo_dado)

    except Exception as e:
        return jsonify({'erro': str(e)}), 500

# Rota GET: O HTML consulta aqui para atualizar a tela
@app.route('/dados', methods=['GET'])
def get_dados():
    return jsonify(ultimo_dado)

if __name__ == '__main__':
    print("Servidor Flask com IA rodando em http://localhost:5000")
    app.run(host='0.0.0.0', port=5000, debug=True)