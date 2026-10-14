function WebPrintService(mode,callback) {
    var websocket = null;
    var callbacklist = [];
    var isready = false;
    var currentport = 0;
    var hasopen = false;
    var connectionStatus = { 'CONNECTING': 0, 'OPEN': 1, 'CLOSING': 2, 'CLOSED': 3 };
    var self = this;
    var started = new Date();
    var noofattempt = 0;
    var maxattempt = 10;
    var hasdisabled = false;
    this.mode = mode;
    this.callback = callback;
    this.ListOfPrinters = [];
    this.Initialize = function (server, fromport, toport) {
        if (currentport == 0) {
            currentport = fromport;
            self.server = server;
            self.fromport = fromport;
            self.toport = toport;
        }
        try {
            webSocket = new WebSocket('ws://' + server + ':' + currentport + '/ws');
            this.ConnectEvents();
        } catch (e) {
            alert('error' + e);
        }
    }

    this.Reconnect = function (reconnect) {
        noofattempt++;
        if (noofattempt > maxattempt) {
            hasdisabled = true;
            return;
        }

        if (reconnect)
            currentport = 0;
        else {
            currentport++;
            if (self.toport < currentport)
                currentport = 0;
        }
        self.Initialize(self.server, self.fromport, self.toport);
    }

    this.ConnectEvents = function () {
        webSocket.onmessage = function (event) {
            var response = JSON.parse(event.data);
            if (response.method == 'ListOfPrinters')
                self.ListOfPrinters = response.data;
            else if(response.method == 'License')
                self.License = response.data;
            if (self.callback)
                self.callback(response)
        };
        webSocket.onopen = function (event) {
            hasopen = true;
            if (self.mode == 'Printer')
                webSocket.send('{"method":"GET","name":"ListOfPrinters"}');
            else
                webSocket.send('{"method":"GET","name":"License"}');
        };
        webSocket.onclose = function (event) {
            setTimeout(self.Reconnect(hasopen), 100);
        };
        webSocket.onerror = function (event) {
        };
    }

    this.Send = function (data) {
        if (webSocket.readyState == connectionStatus.OPEN) {
            webSocket.send(data);
        }
    }
    this.ListOfPrinters = function (callback) {
        //callbacklist.push(callback);
        this.Send('{"method":"GET","name":"ListOfPrinters"}');
    };
    this.License = function (callback) {
        //callbacklist.push(callback);
        this.Send('{"method":"GET","name":"License"}');
    };
    this.PrintRAW = function (document, callback) {
        //callbacklist.push(callback);
        this.Send('{"method":"GET","name":"PrintRAW","document":' + JSON.stringify(document) + '}');
    };
    this.PrintPDF = function (document, callback) {
        //callbacklist.push(callback);
        this.Send('{"method":"GET","name":"PrintPDF","document":' + JSON.stringify(document) + '}');
    };
    this.Print = function (document, callback) {
        //callbacklist.push(callback);
        this.Send('{"method":"GET","name":"Print","document":' + JSON.stringify(document) + '}');
    };
};