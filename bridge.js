var fs = require('fs');
var udp = require("dgram")

// read ssl certificate
var privateKey = fs.readFileSync('/etc/letsencrypt/live/undrfin.de/privkey.pem', 'utf8');
var certificate = fs.readFileSync('/etc/letsencrypt/live/undrfin.de/fullchain.pem', 'utf8');

var credentials = { key: privateKey, cert: certificate };
var https = require('https');

//pass in your credentials to create an https server
var httpsServer = https.createServer(credentials);
httpsServer.listen(19133);

var WebSocketServer = require('ws').Server;
var wss = new WebSocketServer({
    server: httpsServer
});

wss.on('connection', function connection(ws, req) {
    var endpoint = null;
        var port = null;
        var connected = false;
        var client = new udp.createSocket("udp4");
    ws.on('message', function incoming(message) {
            if(endpoint == null) {
                    endpoint = message[0] + "." + message[1] + "." + message[2] + "." + message[3];
                    port = message.readUInt16LE(4)
                            connected = true;
                    console.log(endpoint);
                    return;
            }
            if(connected) {
                    console.log("sending");
                    client.send(message, port, endpoint);
            }
    });
            client.on("message", function(data) {
        //          console.log("recv", data);
                    ws.send(data);
            });

        //console.log(ws);
});
