"use strict";
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:1000/working-management-hub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Start the connection.
start();
function checkAlert() {
    connection.invoke("CheckAlert", "10000").catch(function (err) {
        return console.error(err.toString());
    });
}
function checkServer() {
    if (connection.connectionState === "Connected") {
        setInterval(() => {
            console.log('Kiem tra nhung nhiem vu bi tre sau 30s');
            checkAlert();
        }, 30000);
    }
}
function myStopFunction() {
    clearInterval(intervalSignalr);
}
async function start() {
    try {
        await connection.start();
        console.log("bot connected");
        console.log("State: ", connection.connectionState);
        checkServer();
    } catch (err) {
        console.log(err);
        setTimeout(() => start(), 5000);
    }
};
