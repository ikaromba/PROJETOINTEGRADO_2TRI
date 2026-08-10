let ultimoEstadoMilitar = "";
let valorAdcSimulado = 2000; // valor inicial do teste 

//busca no site
async function buscarDadosAPI() {
    try {
        const resposta = await fetch('http://127.0.0.1:8000/ultimas-medicoes');
        const dados = await resposta.json();
        
        if (dados.length > 0) {
            // pega a medição mais recente para atualizar os visores superiores
            const ultima = dados[0];
            atualizarVisoresPainel(ultima.valor_adc, ultima.classificacao_ia, ultima.horario);
            
            // atualiza a tabela de histórico inteira com base no que veio do banco/memória 
            atualizarTabelaHistorico(dados);
        }
    } catch (erro) {
        console.log("%c[CONEXÃO RADAR] Procurando sinal do servidor Python...", "color: #ff3333");
    }
}


function atualizarVisoresPainel(valor, classificacao, hora) {
    const elementoValor = document.getElementById('valor-atual');
    const elementoHora = document.getElementById('hora-atualizacao');
    const badge = document.getElementById('badge-status');

    if (elementoValor) elementoValor.innerText = `[ ${valor} ]`;
    if (elementoHora) elementoHora.innerText = `SYS_TIME // ${hora}`;
    
    if (badge) {
        badge.innerText = `// ${classificacao.toUpperCase()}`;
        // modifica dinamicamente as classes CSS para as cores de alerta tático
        if (classificacao === "Cheio") {
            badge.className = "badge status-badge status-cheio";
        } else if (classificacao === "Médio") {
            badge.className = "badge status-badge status-medio";
        } else {
            badge.className = "badge status-badge status-baixo";
        }
    }

    // efeito visual de tremor de tela crítico caso mude o status do combustível
    if (ultimoEstadoMilitar !== classificacao && ultimoEstadoMilitar !== "") {
        document.body.classList.add("screen-glitch-active");
        setTimeout(() => document.body.classList.remove("screen-glitch-active"), 400);
    }
    ultimoEstadoMilitar = classificacao;
}


function atualizarTabelaHistorico(listaMedicoes) {
    const tabela = document.getElementById('tabela-historico');
    if (!tabela) return;
    
    tabela.innerHTML = ""; // limpa para rebuildar nítido

    listaMedicoes.forEach(medicao => {
        let corBadgeClass = '';
        if (medicao.classificacao_ia === 'Cheio') corBadgeClass = 'badge-military-success';
        else if (medicao.classificacao_ia === 'Médio') corBadgeClass = 'badge-military-warning';
        else corBadgeClass = 'badge-military-danger';

        const linha = document.createElement('tr');
        linha.innerHTML = `
            <td><i class="bi bi-terminal me-2"></i>${medicao.horario}</td>
            <td class="fw-bold font-monospace">> TRK_ADC_${medicao.valor_adc}</td>
            <td><span class="${corBadgeClass}">${medicao.classificacao_ia.toUpperCase()}</span></td>
        `;
        tabela.appendChild(linha);
    });
}


async function ajustarTanque(variacao) {
    // calcula o novo valor respeitando o limite tático do ADC (0 a 4095)
    valorAdcSimulado += variacao;
    if (valorAdcSimulado > 4095) valorAdcSimulado = 4095;
    if (valorAdcSimulado < 0) valorAdcSimulado = 0;

    // atualiza o texto interno do painel de controle de simulação
    const debugElement = document.getElementById('debug-simulador');
    if (debugElement) {
        debugElement.innerText = `Valor ADC Interno: ${valorAdcSimulado}`;
    }

    // dispara via POST exatamente o payload esperado no server.py
    try {
        await fetch('http://127.0.0.1:8000/medicao', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ valor_adc: valorAdcSimulado })
        });
        
        // atualiza a tela imediatamente após processar sem esperar o relógio do intervalo
        buscarDadosAPI();
    } catch (erro) {
        console.error("[ERRO NO SIMULADOR] Verifique se o server.py está executando.", erro);
    }
}

setInterval(buscarDadosAPI, 1000);