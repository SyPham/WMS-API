"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("http://10.4.0.76:96/working-management-hub").build();
var intervalSignalr = null;
console.log(connection);
connection.start().then(function () {
    console.log("bot connected");
    console.log("State: ", connection.connectionState);
    checkServer();
}).catch(function (err) {
    myStopFunction();
    return console.error(err.toString());
});


function checkAlert() {
    connection.invoke("CheckAlert", "10000").catch(function (err) {
        return console.error(err.toString());
    });
}
function checkServer() {
    if (connection.connectionState === "Connected") {
        intervalSignalr = setInterval(() => {
            console.log('Kiem tra nhung nhiem vu bi tre sau 30s');
            checkAlert();
        }, 30000);
    }
}
function myStopFunction() {
    clearInterval(intervalSignalr);
}