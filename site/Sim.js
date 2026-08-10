// Importando o módulo http
const http = require('http');
const fs = require('fs');
const path = require('path');

// Criando o servidor
const server = http.createServer((req, res) => {
    //<br> para quebrar linha
    //200 sucesso, 404 sevidor nao encontrado, 500 erro no servidor, 1xx informativo, 3xx redirecionamento, 4xx erro do cliente
    if(req.url === '/') {
        res.writeHead(200, {'Content-Type': 'text/html; charset=utf-8'});
        res.end('Bem vindo a pagina inicial <br> Use /Ideia para acessar a pagina de login <br> Use /Primeiro para acessar a pagina do primeiro projeto HTML <br> Use /birb para acessar a pagina do birb <br> Use /Erros para acessar a pagina de erros e status');

    }
    else if(req.url === '/Ideia') {
        const filePath = path.join(__dirname, 'index.html')
        fs.readFile(filePath, (err, content) => {
            if (err){
                res.writeHead(500, {'Content-Type': 'text/html'})
                res.end('Erro interno do servidor');
            } else {
                res.writeHead(200, {'Content-Type': 'text/html'});
                res.end(content);
            }
        });
    }
    else if(req.url === '/Primeiro') {
        const filePath = path.join(__dirname, 'index1.html')
        fs.readFile(filePath, (err, content) => {
            if (err){
                res.writeHead(500, {'Content-Type': 'text/html'})
                res.end('Erro interno do servidor');
            } else {
                res.writeHead(200, {'Content-Type': 'text/html'});
                res.end(content);
            }
        });
    }
    else if(req.url === '/birb') {
        const filePath = path.join(__dirname, 'index2.html')
        fs.readFile(filePath, (err, content) => {
            if (err){
                res.writeHead(500, {'Content-Type': 'text/html'})
                res.end('Erro interno do servidor');
            } else {
                res.writeHead(200, {'Content-Type': 'text/html'});
                res.end(content);
            }
        });
        
    }
    else if(req.url === '/Erros'){
        res.writeHead(200, {'Content-Type': 'text/html'});
        res.end('Status e Erros (BETA) <br> . <br> 1xx(informativo) 2xx(sucesso) 3xx(redirecionamento) 4xx (erro do cliente) 5xx (Erro no seerver) <br> . <br> 400 (Erro do cliente) <br> 404 (Servidor nao encontrado) <br> 500 (Erro no servidor) <br> 403 (O usuario nao tem o nivel de acesso necessario) <br> 401 (Erro quando o servidor nao reconhece quem voce e). <br> . <br> Use / para acessar a pagina inicial');
    }
    else {
    const filePath = path.join(__dirname, 'index404.html');
    fs.readFile(filePath, (err, content) => {
        if (err){
            res.writeHead(500, {'Content-Type': 'text/html'})
            res.end('Erro interno do servidor');
        } else {
            res.writeHead(404, {'Content-Type': 'text/html'});
            res.end(content);
        }
    });

}
});
// Especificando a porta
const port = 3000;
server.listen(port, () => {
console.log(`Servidor rodando em http://localhost:${port}/`);
});