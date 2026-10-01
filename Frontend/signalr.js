window.signalR = (function() {
    function HubConnectionBuilder() {
        this.url = "";
        this.withUrl = function(url) {
            this.url = url;
            return this;
        };
        this.withAutomaticReconnect = function() {
            return this;
        };
        this.build = function() {
            return new HubConnection(this.url);
        };
    }

    function HubConnection(url) {
        var self = this;
        this.url = url;
        this.connectionId = "ID_" + Math.floor(Math.random() * 10000);
        this.handlers = {};

        this.onreconnectingCallback = function() {};
        this.onreconnectedCallback = function() {};
        this.oncloseCallback = function() {};

        this.onreconnecting = function(callback) { this.onreconnectingCallback = callback; };
        this.onreconnected = function(callback) { this.onreconnectedCallback = callback; };
        this.onclose = function(callback) { this.oncloseCallback = callback; };

        this.on = function(methodName, callback) {
            this.handlers[methodName.toLowerCase()] = callback;
        };

        this.start = function() {
            return new Promise(function(resolve, reject) {
                var wsUrl = self.url.replace("http://", "ws://").replace("https://", "wss://") + "/negotiate?negotiateVersion=1";
                
                var directWsUrl = self.url.replace("http://", "ws://").replace("https://", "wss://");
                self.socket = new WebSocket(directWsUrl);

                self.socket.onopen = function() {
                    if (self.handlers["updateonlinecount"]) {
                        self.handlers["updateonlinecount"](1);
                    }
                    resolve();
                };

                self.socket.onerror = function(err) {
                    reject(err);
                };

                self.socket.onmessage = function(event) {
                    try {
                        var message = JSON.parse(event.data);
                        var target = message.target ? message.target.toLowerCase() : "";
                        if (self.handlers[target]) {
                            self.handlers[target].apply(null, message.arguments || []);
                        }
                    } catch (e) {
                    }
                };

                self.socket.onclose = function() {
                    if (self.oncloseCallback) self.oncloseCallback();
                };
            });
        };

        this.invoke = function(methodName, varArgs) {
            var args = Array.prototype.slice.call(arguments, 1);
            if (self.socket && self.socket.readyState === WebSocket.OPEN) {
                self.socket.send(JSON.stringify({
                    target: methodName,
                    arguments: args
                }));
            }
            
            if (methodName === "JoinRoom" && self.handlers["joinedroom"]) {
                self.handlers["joinedroom"](args[0]);
            }
            if (methodName === "LeaveRoom" && self.handlers["leftroom"]) {
                self.handlers["leftroom"]("");
            }
            return Promise.resolve();
        };
    }

    return {
        HubConnectionBuilder: HubConnectionBuilder
    };
})();
