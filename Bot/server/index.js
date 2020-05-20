const express = require('express');
const signalr = require('@microsoft/signalr');
const app = express();
 const http = require('http');

const connection = new signalr.HubConnectionBuilder()
                            .withUrl("http://10.4.4.224:106/working-management-hub")
                                    .build();
        async function start() {
            try {
                await connection.start();
                console.log("connected");
            } catch (err) {
                console.log(err);
                setTimeout(() => start(), 5000);
            }
        };
        connection.onclose(async () => {
            await start();
        });

        // Start the connection.
        start();
const server = http.createServer(httpOption, app).listen(1005,function() {
    console.log('listening on 1005')
  })

