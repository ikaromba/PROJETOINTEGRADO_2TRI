var canvas = document.getElementById("canvas");
canvas.width = 800;
canvas.height = 600;

var c = canvas.getContext("2d");

var P1 = 50;
var P2 = 750;
var barraAltura = 200;
var velocidade = 10;

var bolaX = 400;
var bolaY = 300;
var bolaRaio = 15;
var bolaDX = 4;
var bolaDY = 4;
var teclas = {};

window.addEventListener("keydown", function(evento){
    teclas[evento.key] = true;
});

window.addEventListener("keyup", function(evento){
    teclas[evento.key] = false;
}); 

function rodarJogo() {
    c.clearRect(0, 0, canvas.width, canvas.height);

    if (teclas["W"] || teclas["w"]) { P1 -= velocidade; }
    if (teclas["S"] || teclas["s"]) { P1 += velocidade; }   

    if (teclas["ArrowUp"]) { P2 -= velocidade; }    
    if (teclas["ArrowDown"]) { P2 += velocidade; }  

    if (P1 < 0) { P1 = 0; }
    if (P1 > canvas.height - barraAltura) { P1 = canvas.height - barraAltura; }
    if (P2 < 0) { P2 = 0; }
    if (P2 > canvas.height - barraAltura) { P2 = canvas.height - barraAltura; }

     bolaX += bolaDX; 
    bolaY += bolaDY; 

    if (bolaY - bolaRaio < 0 || bolaY + bolaRaio > canvas.height) {
        bolaDY = -bolaDY;
    }

c.beginPath();
c.arc(400,400,60,0, Math.PI * 2, false);
c.strokeStyle = "#9b05f2";
c.lineWidth = 1;
c.stroke();

 c.beginPath();
    c.moveTo(50, P1);
    c.lineTo(50, P1 + barraAltura);    
    c.lineWidth = 10;
    c.strokeStyle = "#ff0000";
    c.stroke();

     c.beginPath();
    c.moveTo(750, P2);
    c.lineTo(750, P2 + barraAltura);    
    c.lineWidth = 10;
    c.strokeStyle = "#0059ff";
    c.stroke();

    requestAnimationFrame(rodarJogo);
}

rodarJogo();



