function PaymentIntegration(mode, applicationsettings, localStorageService,
    http, timeout, parentscope, rootscope) {
    this.settings = applicationsettings;
    this.scope = parentscope;
    this.rootscope = rootscope;
    this.http = http;
    this.mode = mode;
    this.lock = false;
    this.timeout = timeout;
    this.localStorageService = localStorageService;
    switch (mode) {
        case 1:
            this.Integrater = new innoviti(this);
            break;
        case 2:
            this.Integrater = new razorpay(this);
            break;
        case 3:
            this.Integrater = new pinelab(this);
            break;
        case 4:
            this.Integrater = new paytm(this);
            break;
        case 11:
            this.Integrater = new razerQR(this);
            break;
        case 12:
            this.Integrater = new paytmQR(this);
            break;
    }
    this.CurrentSingleWindowSettlementNo = null;
    this.CurrentSingleWindowSettlementID = null;
    this.IsSingleWindow = false;
    this.SendWebData = function (request, callback) {
        var printerData = this.localStorageService.get('PrinterData');
        if (printerData == null) {
            return;
        }
        this.rootscope.SendWebData(printerData, request, callback)
    };

    this.EnableSingleWindowNo = function (locationid, counterid, callback) {
        this.IsSingleWindow = true;
        this.locationid = locationid;
        this.counterid = counterid;
        var localscope = this;
        localscope.CurrentSingleWindowSettlementID = localStorageService.get('singlewindow_' + locationid + '_' + counterid);
        if (localscope.CurrentSingleWindowSettlementID == undefined ||
            localscope.CurrentSingleWindowSettlementID == null) {
            this.SetNextAutoNumber(callback);
        }
        else {
            localscope.http.get('BillsettlementmasterService/WindowSettlementNumber/' + locationid + '/' + counterid).then(function (results) {
                if (results.data == null ||
                    results.data == '' ||
                    localscope.CurrentSingleWindowSettlementID != results.data) {
                    localscope.http.get('BillsettlementmasterService/NextWindowSettlementNumber/' + locationid + '/' + counterid).then(function (results) {
                        localscope.CurrentSingleWindowSettlementID = results.data;
                        localscope.CurrentSingleWindowSettlementNo = 'SW_' + locationid + '_' + counterid + '_' + results.data;
                        localStorageService.set('singlewindow_' + locationid + '_' + counterid, results.data);
                        if (callback && callback != null)
                            callback();
                    });
                }
                else {
                    localscope.CurrentSingleWindowSettlementNo = 'SW_' + locationid + '_' + counterid + '_' + localscope.CurrentSingleWindowSettlementID;
                    localscope.HasConsumed({ 'id': localscope.CurrentSingleWindowSettlementNo },
                        function (item) {
                            if (item && item != null && item != '') {
                                localscope.SetNextAutoNumber(callback);
                            }
                            else {
                                if (callback && callback != null)
                                    callback();
                            }
                        });
                }
            });
        }
    }

    this.SetNextAutoNumber = function (callback) {
        if (this.IsSingleWindow) {
            var localscope = this;
            localscope.http.get('BillsettlementmasterService/NextWindowSettlementNumber/' + localscope.locationid + '/' + localscope.counterid).then(function (results) {
                localscope.CurrentSingleWindowSettlementID = results.data;
                localscope.CurrentSingleWindowSettlementNo = 'SW_' + localscope.locationid + '_' + localscope.counterid + '_' + results.data;
                localStorageService.set('singlewindow_' + localscope.locationid + '_' + localscope.counterid, results.data);
                if (callback && callback != null)
                    callback();
            });
        }
        else {
            if (callback && callback != null)
                callback();
        }
    }

    this.HasConsumed = function (item, callback) {
        this.http.get('BillsettlementmasterService/HasSettlementNumber/' + this.locationid + '/' + this.counterid + '/' + item.id).then(function (results) {
            if (results.data == null ||
                results.data == '') {
                if (callback) callback(results.data);
            }
        });
    }

    this.ClearSingleWindowNo = function (refid) {
        if (this.IsSingleWindow) {
            localStorageService.set('singlewindow_' + this.locationid + '_' + this.counterid, null);
        }
        if (refid &&
            refid != null) {
            var key = '';
            switch (this.mode) {
                case 1:
                    key = 'innovettihistory';
                    break;
                case 2:
                    key = 'razorhistory';
                    break;
                case 3:
                    key = 'pinelabhistory';
                    break;
                case 4:
                    key = 'paytmhistory';
                    break;
            }
            var data = localStorageService.get(key);
            if (data && data != null) {
                var exist = data.find(function (item) {
                    return item.refid == refid;
                });
                if (exist && exist != null) {
                    data.splice(data.indexOf(exist), 1);
                    localStorageService.set(key, data);
                }
            }
        }
    }

    this.CallBack = null;

    this.SetKeyValue = function (configList, searchKey, objectKey) {
        var keyval = configList.find(function (item) {
            return item.key == searchKey;
        });
        if (keyval &&
            keyval != null) {
            this.deviceconfig[objectKey] = keyval.value;
        }
    }

    this.configChanged = function (id, options) {
        this.deviceconfig = {};
        var deviceconfig = options.find(function (item) {
            return item.id == id;
        });

        if (deviceconfig && deviceconfig != null &&
            deviceconfig.additionalinfo && deviceconfig.additionalinfo != null) {
            var configList = deviceconfig.additionalinfo.payintegration.list;
            this.SetKeyValue(configList, 'POS Code', 'MerchantStorePosCode');
            this.SetKeyValue(configList, 'IMEI', 'IMEI');
            this.SetKeyValue(configList, 'User Name', 'username');
            this.SetKeyValue(configList, 'Password', 'password');
            this.SetKeyValue(configList, 'Security Token', 'SecurityToken');
            this.SetKeyValue(configList, 'App Key', 'appKey');

            this.SetKeyValue(configList, 'Merchant ID', 'MerchantID');
            this.SetKeyValue(configList, 'Device ID', 'DeviceID');
            this.SetKeyValue(configList, 'Channel', 'channelId');
            this.SetKeyValue(configList, 'Version', 'version');
            this.SetKeyValue(configList, 'Access Key', 'accesskey');


            //var keyval = configList.find(function (item) {
            //    return item.key == 'POS Code';
            //});
            //if (keyval &&
            //    keyval != null) {
            //    this.deviceconfig.MerchantStorePosCode = keyval.value;
            //}
            //var keyval = configList.find(function (item) {
            //    return item.key == 'IMEI';
            //});
            //if (keyval &&
            //    keyval != null) {
            //    this.deviceconfig.IMEI = keyval.value;
            //}
            //var keyval = configList.find(function (item) {
            //    return item.key == 'Merchant ID';
            //});
            //if (keyval &&
            //    keyval != null) {
            //    this.deviceconfig.MerchantID = keyval.value;
            //}
            //var keyval = configList.find(function (item) {
            //    return item.key == 'Device ID';
            //});
            //if (keyval &&
            //    keyval != null) {
            //    this.deviceconfig.DeviceID = keyval.value;
            //}

        }
    }

    function razorpay(parent) {
        this.focusid = 'Razerpay_optionid';
        this.name = 'Razor';
        this.parent = parent;
        this.status = '';
        this.timer = null;
        this.payrequest = '';
        this.p2pRequestId = '';
        this.Run = function (deviceid, deviceconfig, optionid, settlement) {
            this.parent.lock = true;
            this.config = (this.parent.settings.int_payconfig &&
                this.parent.settings.int_payconfig != null &&
                this.parent.settings.int_payconfig != '') ? (JSON.parse(this.parent.settings.int_payconfig)) : {};
            this.config.DeviceID = deviceid;
            if (deviceconfig && deviceconfig != null) {
                for (key in deviceconfig) {
                    this.config[key] = deviceconfig[key];
                }
            }
            this.cancelled = false;
            var billnos = [];
            var billids = [];
            if (this.parent.IsSingleWindow) {
                billnos.push(this.parent.CurrentSingleWindowSettlementNo);
                billids.push(this.parent.CurrentSingleWindowSettlementNo);
            }
            else {
                $.each(settlement.Billsettlement, function (k, v) {
                    billnos.push(v.billno);
                    billids.push(v.billno);
                });
            }

            this.parent.CallBack(-1, 'Connecting', this);
            var razorrequest = {
                "appKey": this.config.appKey,
                "username": this.config.username,
                "password": this.config.password,
                "amount": Math.round(settlement.balanceamount).toString(),
                "externalRefNumber": billnos.join('_'),
                "externalRefNumber2": this.parent.scope.UserInfo.id,
                "externalRefNumber3": "",
                "customerEmail": "",
                "mode": optionid,
                "pushTo": {
                    "deviceId": this.config.DeviceID + "|ezetap_android"
                }
            };

            this.IsUPIBoard = false;
            if (optionid == "UPI")
                optionid = "BHARATQR";

            if (optionid == 'UPI BOARD') {
                var upiboardoption = 'razorpay_pos_soundbox';
                if (this.config.upiboard && this.config.upiboard != null &&
                    this.config.upiboard != '') {
                    upiboardoption = this.config.upiboard;
                }
                optionid = 'BHARATQR';
                razorrequest.mode = optionid;
                razorrequest.pushTo.deviceId = this.config.DeviceID + "|" + upiboardoption;
                this.IsUPIBoard = true;
            }

            this.payrequest = JSON.stringify(razorrequest);
            this.deviceid = deviceid;
            this.optionid = optionid;
            var localscope = this;
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }
            var request = {
                'id': printerData.printerid,
                'content': JSON.stringify(razorrequest),
                'mode': 'post',
                'encoded': '',
                'destination': this.config.sendurl
            };
            var srequest = JSON.stringify(request);
            this.parent.SendWebData({
                method: 'POST',
                url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/WebData',
                data: srequest
            }, function (result) {
                if (result.success) {
                    localscope.p2pRequestId = result.p2pRequestId;

                    var history = { 'request': request, 'response': result };
                    var razorhistory = localStorageService.get('razorhistory');
                    if (razorhistory == undefined || razorhistory == null)
                        razorhistory = [];
                    var exist = razorhistory.find(function (item) {
                        return item.externalRefNumber == request.externalRefNumber;
                    });
                    if (exist && exist != null) {
                        exist.request = history.request;
                        exist.response = history.response;
                    }
                    else {
                        razorhistory.push(history);
                    }
                    localStorageService.set('razorhistory', razorhistory);

                    localscope.timeoutid = localscope.parent.timeout(function () {
                        localscope.CheckStatus(localscope, result);
                    }, 100);
                }
                else if (result.realCode == 'P2P_DEVICE_BUSY_WITH_PENDING_NOTIFICATION') {
                    var razorhistory = localStorageService.get('razorhistory');
                    if (razorhistory == undefined || razorhistory == null)
                        razorhistory = [];
                    var exist = razorhistory.find(function (item) {
                        return item.externalRefNumber == request.externalRefNumber;
                    });
                    if (exist && exist != null) {
                        result = exist.response;
                        localscope.p2pRequestId = result.p2pRequestId;
                        localscope.timeoutid = localscope.parent.timeout(function () {
                            localscope.CheckStatus(localscope, result);
                        }, 100);
                    }
                    else {
                        localscope.parent.CallBack(2, result.message, localscope);
                        localscope.parent.ClearSingleWindowNo();
                    }
                }
                else {
                    localscope.parent.CallBack(2, result.message, localscope);
                    localscope.parent.ClearSingleWindowNo();
                }
            });
        }

        this.GetStatus = function () {
            var localscope = this;
            var razorrequest = {
                "appKey": localscope.config.appKey,
                "username": localscope.config.username,
                "origP2pRequestId": localscope.p2pRequestId
            };
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }
            var request = {
                'id': printerData.printerid,
                'content': JSON.stringify(razorrequest),
                'mode': 'post',
                'encoded': '',
                'destination': localscope.config.statusurl
            };
            var srequest = JSON.stringify(request);
            localscope.parent.SendWebData({
                method: 'POST',
                url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/WebData',
                data: srequest
            }, function (result) {
                if (result.messageCode == 'P2P_DEVICE_TXN_DONE') {
                    result = localscope.Convert2Result(localscope, result);
                    localscope.parent.CallBack(3, result, localscope);
                    localscope.parent.ClearSingleWindowNo();
                }
                else {
                    localscope.parent.CallBack(2, result.message, localscope);
                    localscope.parent.ClearSingleWindowNo();
                }
                localscope.p2pRequestId = null;
            });
        }

        this.CheckStatus = function (localscope, response) {
            var razorrequest = {
                "appKey": localscope.config.appKey,
                "username": localscope.config.username,
                "origP2pRequestId": response.p2pRequestId
            };
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }
            var request = {
                'id': printerData.printerid,
                'content': JSON.stringify(razorrequest),
                'mode': 'post',
                'encoded': '',
                'destination': localscope.config.statusurl
            };
            var srequest = JSON.stringify(request);
            localscope.parent.SendWebData({
                method: 'POST',
                url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/WebData',
                data: srequest
            }, function (result) {
                if (result.success) {
                    if (result.messageCode == 'P2P_DEVICE_CANCELED') {
                        localscope.parent.CallBack(2, result.message, localscope);
                    }
                    else if (result.messageCode == 'P2P_DEVICE_TXN_DONE' &&
                        (result.errorMessage == undefined || result.errorMessage == null &&
                            result.errorMessage == '')) {
                        result = localscope.Convert2Result(localscope, result);
                        localscope.parent.CallBack(3, result, localscope);
                        localscope.parent.ClearSingleWindowNo();
                    }
                    else {
                        if (result.errorMessage != undefined &&
                            result.errorMessage != null &&
                            result.errorMessage != '') {
                            localscope.rootscope.Toaster.Error(result.errorMessage);
                        }
                        localscope.parent.CallBack(1, result.message);
                        if (!localscope.cancelled) {
                            localscope.timeoutid = localscope.parent.timeout(function () {
                                localscope.CheckStatus(localscope, response);
                            }, 100);
                        }
                    }
                }
                else {
                    localscope.parent.CallBack(0, result.message, localscope);
                    localscope.parent.ClearSingleWindowNo();
                }
            });
        }

        this.Convert2Result = function (localscope, result) {
            result.FullResponse = JSON.stringify(result);
            result.Request_Input = localscope.payrequest;
            result.paymentuniqueid = localscope.parent.CurrentSingleWindowSettlementNo;
            result.ResponseCode = result.messageCode;
            result.ResponseMessage = result.message;
            result.ApprovalCode = result.p2pRequestId + '-' + result.authCode;
            result.RRN_No = result.rrNumber;
            result.Amount = result.totalAmount;
            result.Card_Num = result.formattedPan;
            result.Card_Type = result.paymentCardBrand;
            result.CardHolder_Name = result.payerName;
            result.Txn_Date = result.chargeSlipDate;
            result.Txn_Type = result.paymentMode;
            result.BankInvoice_Num = result.txnId;
            result.Batch_Number = result.pgInvoiceNumber;
            result.Terminal_Id = result.deviceSerial;
            result.Merchant_Id = result.merchantCode;
            result.PaymentMode = result.paymentMode.toUpperCase();
            return result;
        }

        this.Cancel = function () {
            this.cancelled = true;

            if (this.timeoutid && this.timeoutid != null)
                this.parent.timeout.cancel(this.timeoutid);

            //this.config = (this.parent.settings.int_payconfig &&
            //    this.parent.settings.int_payconfig != null &&
            //    this.parent.settings.int_payconfig != '') ? (JSON.parse(this.parent.settings.int_payconfig)) : {};

            var razorrequest = {
                "appKey": this.config.appKey,
                "username": this.config.username,
                "origP2pRequestId": this.p2pRequestId,
                "pushTo": {
                    "deviceId": this.config.DeviceID + "|ezetap_android"
                }
            };

            if (this.IsUPIBoard == true) {
                razorrequest.pushTo.deviceId = this.config.DeviceID + "|razorpay_pos_soundbox";

                var upiboardoption = 'razorpay_pos_soundbox';
                if (this.config.upiboard && this.config.upiboard != null &&
                    this.config.upiboard != '') {
                    razorrequest.pushTo.deviceId = this.config.DeviceID + "|" + this.config.upiboard;
                }
            }

            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }
            var localscope = this;
            var request = {
                'id': printerData.printerid,
                'content': JSON.stringify(razorrequest),
                'mode': 'post',
                'encoded': '',
                'destination': localscope.config.cancelurl
            };
            var srequest = JSON.stringify(request);
            localscope.parent.SendWebData({
                method: 'POST',
                url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/WebData',
                data: srequest
            }, function (result) {
                if (result.success) {
                    localscope.parent.CallBack(2, result.message, localscope);
                }
                else {
                    localscope.parent.CallBack(0, result.message, localscope);
                }
            });
        }

        this.Options = [
            { 'id': 'ALL', 'name': 'ALL' },
            { 'id': 'CARD', 'name': 'CARD' },
            { 'id': 'UPI', 'name': 'UPI' },
            { 'id': 'BHARATQR', 'name': 'BHARATQR' },
            { 'id': 'UPI BOARD', 'name': 'UPI BOARD' }
        ];
    }

    function pinelab(parent) {
        this.focusid = 'Pinelab_optionid';
        this.name = 'Pinelab';
        this.parent = parent;
        this.status = '';
        this.timer = null;
        this.PlutusTransactionReferenceID = '';
        this.UserID = '';
        this.Run = function (deviceid, deviceconfig, optionid, settlement) {
            this.parent.lock = true;
            this.cancelled = false;
            this.config = (this.parent.settings.int_payconfig &&
                this.parent.settings.int_payconfig != null &&
                this.parent.settings.int_payconfig != '') ? (JSON.parse(this.parent.settings.int_payconfig)) : {};

            this.config.IMEI = deviceid;
            if (deviceconfig && deviceconfig != null) {
                for (key in deviceconfig) {
                    this.config[key] = deviceconfig[key];
                }
            }

            var billnos = [];
            var billids = [];
            if (this.parent.IsSingleWindow) {
                billnos.push(this.parent.CurrentSingleWindowSettlementNo);
                billids.push(this.parent.CurrentSingleWindowSettlementNo);
            }
            else {
                $.each(settlement.Billsettlement, function (k, v) {
                    billnos.push(v.billno);
                    billids.push(v.billno);
                });
            }
            this.parent.CallBack(-1, 'Connecting', this);
            this.UserID = this.parent.scope.UserInfo.id;

            //this.config.MerchantStorePosCode

            var pinerequest = {
                "TransactionNumber": billids.join('_'),
                "SequenceNumber": billids.length,
                "AllowedPaymentMode": optionid,
                "MerchantStorePosCode": this.config.MerchantStorePosCode,
                "Amount": Math.round(settlement.balanceamount * 100).toString(),
                "InvoiceNumber": billnos.join('_'),
                "UserID": "Quanto-" + this.UserID,
                "MerchantID": this.config.MerchantID,
                "SecurityToken": this.config.SecurityToken,
                "IMEI": this.config.IMEI,
                "AutoCancelDurationInMinutes": this.config.AutoCancelDurationInMinutes
            };
            this.Request = pinerequest;
            this.deviceid = deviceid;
            this.optionid = optionid;
            this.amount = Math.round(settlement.balanceamount * 100);
            var localscope = this;
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }
            var request = {
                'id': printerData.printerid,
                'content': JSON.stringify(pinerequest),
                'mode': 'post',
                'encoded': '',
                'destination': this.config.sendurl
            };
            this.ManualSettlement = false;
            var srequest = JSON.stringify(request);
            this.parent.SendWebData({
                method: 'POST',
                url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/WebData',
                data: srequest
            }, function (result) {
                if (result.ResponseMessage &&
                    result.ResponseMessage == 'APPROVED') {

                    var history = {
                        'request': localscope.Request,
                        'response': result,
                        'name': localscope.Request.Amount.toString() + "(" +
                            localscope.Request.InvoiceNumber + ")",
                        'id': localscope.Request.InvoiceNumber,
                        'refid': result.PlutusTransactionReferenceID
                    };

                    var pinelabhistory = localStorageService.get('pinelabhistory');
                    if (pinelabhistory == undefined || pinelabhistory == null)
                        pinelabhistory = [];
                    var exist = pinelabhistory.find(function (item) {
                        return item.TransactionNumber == localscope.Request.TransactionNumber &&
                            item.InvoiceNumber == localscope.Request.InvoiceNumber &&
                            item.Amount == localscope.Request.Amount;
                    });
                    if (exist && exist != null) {
                        exist.request = history.request;
                        exist.response = history.response;
                        exist.name = history.name;
                        exist.id = history.id;
                        exist.refid = history.refid;
                    }
                    else {
                        pinelabhistory.push(history);
                    }
                    localStorageService.set('pinelabhistory', pinelabhistory);
                    localscope.PlutusTransactionReferenceID = result.PlutusTransactionReferenceID;
                    localscope.timeoutid = localscope.parent.timeout(function () {
                        localscope.CheckStatus(localscope, result);
                    }, 100);
                }
                else if (result.ResponseMessage == 'TXN WITH SIMILAR PARAMS IS ALREADY IN PROGRESS') {
                    localscope.FindSimilar(request, result);
                }
                else if (result.ResponseMessage == 'TXN ALREADY COMPLETED') {
                    if (!localscope.FindSimilar(request, result)) {
                        localscope.EnableManualSettlement();
                    }
                }
                else {
                    localscope.parent.CallBack(2, result.ResponseMessage, localscope);
                    localscope.parent.ClearSingleWindowNo();
                }
            });
        }

        this.FindSimilar = function (request, result) {
            var localscope = this;
            var pinelabhistory = localStorageService.get('pinelabhistory');
            if (pinelabhistory == undefined || pinelabhistory == null)
                pinelabhistory = [];
            var exist = pinelabhistory.find(function (item) {
                return item.TransactionNumber == request.TransactionNumber &&
                    item.InvoiceNumber == request.InvoiceNumber &&
                    item.Amount == request.Amount;
            });
            if (exist && exist != null) {
                result = exist.response;
                localscope.PlutusTransactionReferenceID = result.PlutusTransactionReferenceID;
                localscope.timeoutid = localscope.parent.timeout(function () {
                    localscope.CheckStatus(localscope, result);
                }, 100);
                return true;
            }
            else {
                localscope.parent.CallBack(2, result.ResponseMessage, localscope);
                localscope.parent.ClearSingleWindowNo();
                return false;
            }
        }
        this.ManualSettlement = false;
        this.ManualItemHistory = [];
        this.EnableManualSettlement = function () {
            this.ManualItemHistory = [];
            var data = localStorageService.get('pinelabhistory');
            if (data && data != null) {
                this.ManualSettlement = true;
                this.ManualItemHistory = data;
            }
        }

        this.SettleManualy = function (id, settlement) {
            var manualitem = this.ManualItemHistory.find(function (item) {
                return item.id == id
            });

            if (manualitem && manualitem != null) {
                var localscope = this;
                localscope.parent.HasConsumed(manualitem, function (item) {
                    if (item && item != null && item != '') {
                        localscope.rootscope.Toaster.Error(item);
                    }
                    else {
                        var request = item.request;
                        var result = {};
                        result.paymentuniqueid = item.id;
                        result.ResponseCode = "MANUAL";
                        result.ResponseMessage = "MANUAL";
                        result.ApprovalCode = "MANUAL";
                        result.RRN_No = "MANUAL";
                        result.Amount = request.Amount;
                        result.Card_Num = "MANUAL";
                        result.Card_Type = "MANUAL";
                        result.CardHolder_Name = "MANUAL";
                        result.Txn_Date = new Date();
                        var paymode = localscope.Options.find(function (item) {
                            return item.id == localscope.optionid;
                        });
                        result.Txn_Type = paymode ? paymode.mode : 'CARD';
                        result.BankInvoice_Num = request.Amount;
                        result.Batch_Number = "";
                        result.Terminal_Id = localscope.deviceid;
                        result.Merchant_Id = localscope.config.MerchantID;
                        result.PaymentMode = paymode ? paymode.mode : 'CARD';;
                        localscope.parent.CallBack(3, result, localscope);
                        localscope.parent.ClearSingleWindowNo();
                    }
                });
            }


        }

        this.GetStatus = function () {
            var localscope = this;
            var pinerequest = {
                "MerchantStorePosCode": this.config.MerchantStorePosCode,
                "UserID": localscope.UserID,
                "MerchantID": localscope.config.MerchantID,
                "SecurityToken": localscope.config.SecurityToken,
                "IMEI": localscope.config.IMEI,
                "PlutusTransactionReferenceID": localscope.PlutusTransactionReferenceID
            };
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }
            var request = {
                'id': printerData.printerid,
                'content': JSON.stringify(pinerequest),
                'mode': 'post',
                'encoded': '',
                'destination': localscope.config.statusurl
            };
            var srequest = JSON.stringify(request);
            localscope.parent.SendWebData({
                method: 'POST',
                url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/WebData',
                data: srequest
            }, function (result) {
                if (result.ResponseMessage && result.ResponseCode && result.ResponseCode == 0) {
                    if (result.ResponseMessage == 'TXN APPROVED') {
                        result = localscope.Convert2Result(localscope, result);
                        localscope.parent.CallBack(3, result, localscope);
                        localscope.parent.ClearSingleWindowNo(result.PlutusTransactionReferenceID);
                    }
                    else {
                        localscope.parent.CallBack(2, result.ResponseMessage, localscope);
                        localscope.parent.ClearSingleWindowNo();
                    }
                }
                else {
                    localscope.parent.CallBack(2, result.ResponseMessage, localscope);
                    localscope.parent.ClearSingleWindowNo();
                }
                localscope.PlutusTransactionReferenceID = null;
            });
        }

        this.CheckStatus = function (localscope, response) {
            var pinerequest = {
                "MerchantStorePosCode": localscope.config.MerchantStorePosCode,
                "UserID": localscope.UserID,
                "MerchantID": localscope.config.MerchantID,
                "SecurityToken": localscope.config.SecurityToken,
                "IMEI": localscope.config.IMEI,
                "PlutusTransactionReferenceID": localscope.PlutusTransactionReferenceID
            };
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }
            var request = {
                'id': printerData.printerid,
                'content': JSON.stringify(pinerequest),
                'mode': 'post',
                'encoded': '',
                'destination': localscope.config.statusurl
            };
            var srequest = JSON.stringify(request);
            localscope.parent.SendWebData({
                method: 'POST',
                url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/WebData',
                data: srequest
            }, function (result) {
                if (result.ResponseMessage) {
                    if (result.ResponseMessage == 'TXN APPROVED') {
                        result = localscope.Convert2Result(localscope, result);
                        localscope.parent.CallBack(3, result, localscope);
                        localscope.parent.ClearSingleWindowNo(result.PlutusTransactionReferenceID);
                    }
                    else if (result.ResponseCode == 1) {
                        localscope.parent.CallBack(2, result.ResponseMessage);
                        localscope.parent.ClearSingleWindowNo(result.PlutusTransactionReferenceID);
                        localscope.parent.SetNextAutoNumber();
                    }
                    else {
                        localscope.parent.CallBack(1, result.ResponseMessage);
                        if (localscope.cancelled == false) {
                            localscope.timeoutid = localscope.parent.timeout(function () {
                                localscope.CheckStatus(localscope, response);
                            }, 100);
                        }
                    }
                }
                else {
                    localscope.parent.CallBack(0, result.message, localscope);
                    localscope.PlutusTransactionReferenceID = '';
                    localscope.parent.ClearSingleWindowNo();
                }
            });
        }


        this.Cancel = function () {

            this.cancelled = true;
            if (this.timeoutid && this.timeoutid != null)
                this.parent.timeout.cancel(this.timeoutid);

            //this.config = (this.parent.settings.int_payconfig &&
            //    this.parent.settings.int_payconfig != null &&
            //    this.parent.settings.int_payconfig != '') ? (JSON.parse(this.parent.settings.int_payconfig)) : {};
            var localscope = this;
            localscope.ManualSettlement = false;
            var pinerequest = {
                "MerchantStorePosCode": localscope.config.MerchantStorePosCode,
                "MerchantID": localscope.config.MerchantID,
                "SecurityToken": localscope.config.SecurityToken,
                "IMEI": localscope.config.IMEI,
                "PlutusTransactionReferenceID": localscope.PlutusTransactionReferenceID,
                "Amount": localscope.amount
            };
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }
            var localscope = this;
            var request = {
                'id': printerData.printerid,
                'content': JSON.stringify(pinerequest),
                'mode': 'post',
                'encoded': '',
                'destination': localscope.config.cancelurl
            };
            var srequest = JSON.stringify(request);
            localscope.parent.SendWebData({
                method: 'POST',
                url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/WebData',
                data: srequest
            }, function (result) {
                if (result.ResponseCode && result.ResponseCode == 0) {
                    localscope.parent.CallBack(2, result.ResponseMessage, localscope);
                    localscope.PlutusTransactionReferenceID = null;
                }
                else {
                    localscope.parent.CallBack(0, result.ResponseMessage, localscope);
                }
                localscope.parent.ClearSingleWindowNo();
                localscope.parent.SetNextAutoNumber();
            });
        }

        this.GetPaymentMode = function (key) {
            if (key == "CARD") return "CARD";
            if (key == "BRAND EMI" || key == "BANK EMI") return "EMI";
            var upilist = ["PHONEPE", "AIRTEL BANK", "UPI BHARAT QR", "AMAZON PAY", "UPI SALE", "UPI PAYTIM"];
            var exist = upilist.find(function (item) {
                return item == key;
            });
            if (exist) return "UPI";
            return "CASH";
        }

        this.Convert2Result = function (localscope, result) {
            var kv = {};
            $.each(result.TransactionData, function (k, v) {
                kv[v.Tag] = v.Value;
            });
            result.FullResponse = JSON.stringify(result);
            result.paymentuniqueid = localscope.parent.CurrentSingleWindowSettlementNo;
            result.Request_Input = kv.payrequest;
            result.ResponseCode = result.ResponseCode;
            result.ResponseMessage = result.ResponseMessage;
            result.ApprovalCode = kv.ApprovalCode;
            result.RRN_No = kv.RRN;
            result.Amount = kv.FinalAmount;
            result.Card_Num = kv['Card Number'];
            result.Card_Type = kv['Card Type'];
            result.CardHolder_Name = kv['Card Holder Name'];
            result.Txn_Date = kv['Transaction Date'];
            result.Txn_Type = kv.PaymentMode;
            result.BankInvoice_Num = kv['Invoice Number'];
            result.Batch_Number = kv['TransactionLogId'];
            result.Terminal_Id = kv.TID;
            result.Merchant_Id = localscope.config.MerchantID;
            result.PaymentMode = this.GetPaymentMode(kv.PaymentMode);
            return result;
        }

        this.Options = [
            { 'id': '1', 'name': ' Card', 'mode': 'CARD' },
            //{ 'id': '2', 'name': ' Cash', 'mode': 'CASH' },
            //{ 'id': '3', 'name': ' Points', 'mode': 'CARD' },
            //{ 'id': '4', 'name': ' Wallets', 'mode': 'CARD' },
            //{ 'id': '6', 'name': ' Brand EMI', 'mode': 'CARD' },
            //{ 'id': '7', 'name': ' Sodexo', 'mode': 'CARD' },
            { 'id': '8', 'name': ' PhonePe', 'mode': 'UPI' },
            { 'id': '9', 'name': ' UPI PayTm', 'mode': 'UPI' },
            { 'id': '10', 'name': ' UPI Sale', 'mode': 'UPI' },
            { 'id': '11', 'name': ' UPI Bharat QR', 'mode': 'UPI' },
            { 'id': '12', 'name': ' Airtel Bank', 'mode': 'UPI' },
            //{ 'id': '19', 'name': ' Paper POS', 'mode': 'CARD' },
            //{ 'id': '20', 'name': ' Bank EMI', 'mode': 'CARD' },
            { 'id': '21', 'name': ' Amazon Pay', 'mode': 'UPI' }
        ];
    }

    function innoviti(parent) {
        this.focusid = 'Innoviti_optionid';
        this.name = 'Innoviti';
        this.parent = parent;
        this.Run = function (optionid, settlement) {

            this.optionid = optionid;
            var port = localStorageService.get('LocalData.int_payport');
            if (typeof (port) == 'undefined' || port == null) {
                port = (this.parent.settings.int_payconfig &&
                    this.parent.settings.int_payconfig != null &&
                    this.parent.settings.int_payconfig != '') ? this.parent.settings.int_payconfig : 8080;
            }
            var urlprefix = '';

            if (!(port && port != null && port != '')) {
                port = this.parent.settings.int_payport;
                urlprefix = 'http://localhost:' + port + '/innoweb/api/MSwipe?value=';
            }
            else {
                if (port.toString().startsWith('http')) {
                    urlprefix = port + (port.toString().endsWith('/') ? '' : '/') + 'innoweb/api/MSwipe?value=';
                }
                else {
                    urlprefix = 'http://localhost:' + port + '/innoweb/api/MSwipe?value=';
                }
            }

            var options = [];
            options.push('00');
            options.push(optionid);
            var billnos = [];
            $.each(settlement.Billsettlement, function (k, v) {
                billnos.push(v.billno);
            });
            options.push(billnos.join('_') + '!' + this.parent.scope.UserInfo.id + '!' + settlement.customermobileno);
            options.push(Math.round(settlement.balanceamount * 100));
            var url = urlprefix + options.join(',');
            var localscope = this;
            this.parent.CallBack(-1, 'Connecting', this);
            this.parent.SendWebData({ 'url': url, 'method': 'GET', data: '' }, function (result) {
                if (result.ResponseCode == '00') {
                    localscope.parent.CallBack(4, result.ResponseMessage, localscope);
                }
                else {
                    localscope.parent.CallBack(2, result.ResponseMessage, localscope);
                }
            }, function (error) {

                if (error.status == -1) {
                    localscope.parent.CallBack(2, 'Please connect the device', localscope);
                }
                else {
                    localscope.parent.CallBack(2, 'Please connect the device, status - ' + error.status, localscope);
                }
            });
        }

        this.Options = [
            { 'id': '00', 'name': 'EFT (Cr/Dr Transaction)' },
            { 'id': '01', 'name': 'Loyalty' },
            { 'id': '02', 'name': 'Gift Card / Prepaid' },
            { 'id': '03', 'name': 'Gift Voucher' },
            { 'id': '05', 'name': 'Credit Card Points Redemption' },
            { 'id': '10', 'name': 'EMI' },
            { 'id': '11', 'name': 'Brand EMI' },
            { 'id': '12', 'name': 'Bajaj EMI' },
            { 'id': '13', 'name': 'Debit EMI' },
            { 'id': '17', 'name': 'Google-pay' },
            { 'id': '86', 'name': 'UPI' },
            { 'id': '18', 'name': 'PhonePe' },
        ];
    }


    function paytm(parent) {
        this.focusid = 'paytm_optionid';
        this.name = 'Paytm';
        this.parent = parent;
        this.status = '';
        this.timer = null;
        this.payrequest = '';
        this.p2pRequestId = '';
        this.GetCheckSum = function (key, data, callback) {
            this.parent.http({
                method: 'POST',
                url: "ConfigurationService/Checksum",
                data: { 'Item': { 'type': 'paytm', 'key': key, 'data': data } }
            }).success(function (result) {
                callback(result);
            }).error(function (result) {

            });
        };
        this.Run = function (deviceid, deviceconfig, optionid, settlement) {
            localStorageService.set('swippingintegeration', { 'optionid': optionid, 'deviceid': deviceid });
            this.parent.lock = true;
            this.config = (this.parent.settings.int_payconfig &&
                this.parent.settings.int_payconfig != null &&
                this.parent.settings.int_payconfig != '') ? (JSON.parse(this.parent.settings.int_payconfig)) : {};
            this.config.DeviceID = deviceid;
            if (deviceconfig && deviceconfig != null) {
                for (key in deviceconfig) {
                    this.config[key] = deviceconfig[key];
                }
            }
            this.cancelled = false;
            var billnos = [];
            var billids = [];
            if (this.parent.IsSingleWindow) {
                billnos.push(this.parent.CurrentSingleWindowSettlementNo);
                billids.push(this.parent.CurrentSingleWindowSettlementNo);
            }
            else {
                $.each(settlement.Billsettlement, function (k, v) {
                    billnos.push(v.billno);
                    billids.push(v.billno);
                });
            }

            this.parent.CallBack(-1, 'Connecting', this);
            var requesttime = new Date();
            requesttime = requesttime.getFullYear() + '-' +
                (requesttime.getMonth() + 1).toString().padStart(2, '0') + '-' +
                requesttime.getDate().toString().padStart(2, '0') + ' ' +
                requesttime.getHours().toString().padStart(2, '0') +
                ':' + requesttime.getMinutes().toString().padStart(2, '0') + ':' +
                requesttime.getSeconds().toString().padStart(2, '0');

            var transactionid = billnos.join('_').replace(/[^a-zA-Z0-9]/g, 'XxX');
            var paytmrequest =
            {
                "head":
                {
                    "requestTimeStamp": requesttime, "channelId": this.config.channelId, "checksum": "", "version": this.config.version
                },
                "body":
                {
                    "paytmMid": this.config.MerchantID,
                    "paytmTid": this.config.DeviceID,
                    "transactionDateTime": requesttime,
                    "merchantTransactionId": transactionid,
                    "merchantReferenceNo": billnos.join('_'),
                    "transactionAmount": (settlement.balanceamount * 100).round(2).toString(),
                    "merchantExtendedInfo": { "PaymentMode": optionid, "autoAccept": "true" }
                }
            }
            this.merchantTransactionId = transactionid;
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }

            var localscope = this;
            this.GetCheckSum(this.config.accesskey, JSON.stringify(paytmrequest.body), function (checksumresult) {
                paytmrequest.head.checksum = checksumresult.checksum;
                localscope.paytmrequest = paytmrequest;
                localscope.payrequest = JSON.stringify(paytmrequest);
                localscope.deviceid = deviceid;
                localscope.optionid = optionid;
                var request = {
                    'id': printerData.printerid,
                    'content': JSON.stringify(paytmrequest),
                    'mode': 'post',
                    'encoded': '',
                    'destination': localscope.config.sendurl
                };
                var srequest = JSON.stringify(request);
                localscope.parent.SendWebData({
                    method: 'POST',
                    url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/WebData',
                    data: srequest
                }, function (result) {
                    if (result && result.body.resultInfo.resultCode == 'A') {
                        localscope.paytmresult = result;
                        var history = {
                            'request': request, 'response': result,
                            'refid': result.body.merchantTransactionId,
                            'merchantTransactionId': result.body.merchantTransactionId
                        };
                        var paytmhistory = localStorageService.get('paytmhistory');
                        if (paytmhistory == undefined || paytmhistory == null)
                            paytmhistory = [];
                        var exist = paytmhistory.find(function (item) {
                            return item.merchantTransactionId == history.merchantTransactionId;
                        });
                        if (exist && exist != null) {
                            exist.request = history.request;
                            exist.response = history.response;
                        }
                        else {
                            paytmhistory.push(history);
                        }
                        localStorageService.set('paytmhistory', paytmhistory);
                        localscope.timeoutid = localscope.parent.timeout(function () {
                            localscope.CheckStatus(localscope, result);
                        }, 1500);
                    }
                    else if (result && result.body.resultInfo.resultCode == 'F' &&
                        (result.body.resultInfo.resultCodeId == '0333' || result.body.resultInfo.resultCodeId == '0233')) {
                        localscope.parent.ClearSingleWindowNo(result.body.merchantTransactionId);
                        localscope.parent.SetNextAutoNumber();
                        localscope.parent.CallBack(2, 'Please try again', localscope);
                        //var paytmhistory = localStorageService.get('paytmhistory');
                        //if (paytmhistory == undefined || paytmhistory == null)
                        //    paytmhistory = [];
                        //var exist = paytmhistory.find(function (item) {
                        //    return item.merchantTransactionId == result.body.merchantTransactionId;
                        //});
                        //if (exist && exist != null) {
                        //    result = exist.response;
                        //    localscope.merchantTransactionId = result.body.merchantTransactionId;
                        //    localscope.paytmresult = result;
                        //    localscope.timeoutid = localscope.parent.timeout(function () {
                        //        localscope.CheckStatus(localscope, result);
                        //    }, 1000);
                        //}
                        //else {
                        //    localscope.parent.CallBack(2, result.body.resultInfo.resultMsg, localscope);
                        //    localscope.parent.ClearSingleWindowNo();
                        //}
                    }
                    else {
                        localscope.parent.CallBack(2, result.body.resultInfo.resultMsg, localscope);
                        localscope.parent.ClearSingleWindowNo();
                    }
                });
            });
        },

        this.GetStatus = function () {
                var localscope = this;
                var printerData = localStorageService.get('PrinterData');
                if (printerData == null) {
                    return;
                }

                var requesttime = new Date();
                requesttime = requesttime.getFullYear() + '-' +
                    (requesttime.getMonth() + 1).toString().padStart(2, '0') + '-' +
                    requesttime.getDate().toString().padStart(2, '0') + ' ' +
                    requesttime.getHours().toString().padStart(2, '0') +
                    ':' + requesttime.getMinutes().toString().padStart(2, '0') + ':' +
                    requesttime.getSeconds().toString().padStart(2, '0');
                var paytmrequest =
                {
                    "head":
                    {
                        "requestTimeStamp": requesttime, "channelId": localscope.config.channelId, "checksum": "", "version": localscope.config.version
                    },
                    "body":
                    {
                        "paytmMid": localscope.config.MerchantID,
                        "paytmTid": localscope.config.DeviceID,
                        "transactionDateTime": requesttime,
                        "merchantTransactionId": localscope.merchantTransactionId,
                    }
                }

                this.GetCheckSum(localscope.config.accesskey, JSON.stringify(paytmrequest.body), function (checksumresult) {
                    paytmrequest.head.checksum = checksumresult.checksum;
                    var request = {
                        'id': printerData.printerid,
                        'content': JSON.stringify(paytmrequest),
                        'mode': 'post',
                        'encoded': '',
                        'destination': localscope.config.statusurl
                    };
                    var srequest = JSON.stringify(request);
                    localscope.parent.SendWebData({
                        method: 'POST',
                        url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/WebData',
                        data: srequest
                    }, function (result) {
                        if (result && result.body.resultInfo.resultCode == 'A') {
                            result = localscope.Convert2Result(localscope, result);
                            localscope.parent.CallBack(3, result, localscope);
                            localscope.parent.ClearSingleWindowNo();
                        }
                        else {
                            localscope.parent.CallBack(2, result.body.resultInfo.resultMsg, localscope);
                            localscope.parent.ClearSingleWindowNo();
                        }
                        localscope.merchantTransactionId = null;
                    });
                });
            }

        this.CheckStatus = function (localscope, response) {
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }
            var localscope = this;
            var requesttime = new Date();
            requesttime = requesttime.getFullYear() + '-' +
                (requesttime.getMonth() + 1).toString().padStart(2, '0') + '-' +
                requesttime.getDate().toString().padStart(2, '0') + ' ' +
                requesttime.getHours().toString().padStart(2, '0') +
                ':' + requesttime.getMinutes().toString().padStart(2, '0') + ':' +
                requesttime.getSeconds().toString().padStart(2, '0');
            var paytmrequest =
            {
                "head":
                {
                    "requestTimeStamp": requesttime,
                    "channelId": localscope.config.channelId,
                    "checksum": "",
                    "version": localscope.config.version
                },
                "body":
                {
                    "paytmMid": localscope.config.MerchantID,
                    "paytmTid": localscope.config.DeviceID,
                    "transactionDateTime": requesttime,
                    "merchantTransactionId": localscope.merchantTransactionId,
                }
            }
            this.GetCheckSum(localscope.config.accesskey, JSON.stringify(paytmrequest.body), function (checksumresult) {
                paytmrequest.head.checksum = checksumresult.checksum;
                var request = {
                    'id': printerData.printerid,
                    'content': JSON.stringify(paytmrequest),
                    'mode': 'post',
                    'encoded': '',
                    'destination': localscope.config.statusurl
                };
                var srequest = JSON.stringify(request);
                localscope.parent.SendWebData({
                    method: 'POST',
                    url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/WebData',
                    data: srequest
                }, function (result) {
                    if (result && result.body.resultInfo.resultCode == 'P') {
                        if (!localscope.cancelled) {
                            localscope.timeoutid = localscope.parent.timeout(function () {
                                localscope.CheckStatus(localscope, response);
                            }, 500);
                        }
                    } else if (result && result.body.resultInfo.resultCode == 'F') {
                        if (result.body.resultInfo.resultCodeId == '0404') {
                            localscope.parent.ClearSingleWindowNo(result.body.merchantTransactionId);
                            localscope.parent.SetNextAutoNumber();
                            localscope.parent.CallBack(2, 'Transaction cancelled on device/bank, try again', localscope);
                        }
                        else {
                            localscope.parent.CallBack(2, result.body.resultInfo.resultMsg, localscope);
                        }
                    }
                    else if (result && result.body.resultInfo.resultCode == 'S') {
                        result = localscope.Convert2Result(localscope, result);
                        localscope.parent.CallBack(3, result, localscope);
                        localscope.parent.ClearSingleWindowNo();
                    }
                });

            });
        }

        this.Convert2Result = function (localscope, result) {
            if (result.body.payMethod == undefined || result.body.payMethod == null) {
                if (typeof (localscope.paytmrequest.body.merchantExtendedInfo.PaymentMode) != 'undefined')
                    result.body.payMethod = localscope.paytmrequest.body.merchantExtendedInfo.PaymentMode;
            }

            if (result.body.payMethod && result.body.payMethod != null &&
                result.body.payMethod.indexOf('CARD') >= 0)
                result.body.payMethod = 'CARD';

            result.FullResponse = JSON.stringify(result);
            result.Request_Input = localscope.payrequest;
            result.paymentuniqueid = localscope.parent.CurrentSingleWindowSettlementNo;
            result.ResponseCode = result.body.resultInfo.resultCodeId;
            result.ResponseMessage = result.body.resultInfo.resultMsg;
            result.ApprovalCode = result.body.merchantTransactionId + '-' + result.body.authCode;
            result.RRN_No = result.body.retrievalReferenceNo;
            result.Amount = result.body.transactionAmount;
            result.Card_Num = result.body.issuerMaskCardNo;
            result.Card_Type = result.body.cardScheme + '-' + result.body.cardType;
            result.CardHolder_Name = result.body.payerName;
            result.Txn_Date = result.body.transactionDateTime;
            result.Txn_Type = result.body.payMethod;
            result.Txn_Type = result.body.Txn_ID;
            result.BankInvoice_Num = result.body.acquirementId;
            result.Batch_Number = result.body.bankMid + '-' + result.body.bankTid;
            result.Terminal_Id = result.body.paytmTid;
            result.Merchant_Id = result.body.paytmMid;
            result.PaymentMode = result.body.payMethod;
            return result;
        }

        this.Cancel = function () {
            this.cancelled = true;

            if (this.timeoutid && this.timeoutid != null)
                this.parent.timeout.cancel(this.timeoutid);


            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }
            var localscope = this;
            var requesttime = new Date();
            requesttime = requesttime.getFullYear() + '-' +
                (requesttime.getMonth() + 1).toString().padStart(2, '0') + '-' +
                requesttime.getDate().toString().padStart(2, '0') + ' ' +
                requesttime.getHours().toString().padStart(2, '0') +
                ':' + requesttime.getMinutes().toString().padStart(2, '0') + ':' +
                requesttime.getSeconds().toString().padStart(2, '0');

            var paytmrequest =
            {
                "head":
                {
                    "requestTimeStamp": requesttime,
                    "channelId": localscope.config.channelId,
                    "checksum": "",
                    "version": localscope.config.version
                },
                "body":
                {
                    "paytmMid": localscope.config.MerchantID,
                    "paytmTid": localscope.config.DeviceID,
                    "merchantTransactionId": localscope.merchantTransactionId,
                    "transactionDateTime": requesttime
                }
            }
            this.GetCheckSum(localscope.config.accesskey, JSON.stringify(paytmrequest.body), function (checksumresult) {
                paytmrequest.head.checksum = checksumresult.checksum;
                var request = {
                    'id': printerData.printerid,
                    'content': JSON.stringify(paytmrequest),
                    'mode': 'post',
                    'encoded': '',
                    'destination': localscope.config.cancelurl
                };
                var srequest = JSON.stringify(request);
                localscope.parent.SendWebData({
                    method: 'POST',
                    url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/WebData',
                    data: srequest
                }, function (result) {
                    localscope.parent.SetNextAutoNumber();
                    localscope.parent.ClearSingleWindowNo(paytmrequest.body.merchantTransactionId);
                    if (result && result.body.resultInfo.resultCode == 'F') {
                        localscope.parent.CallBack(2, result.body.resultInfo.resultMsg, localscope);
                    }
                    else if (result && result.body.resultInfo.resultCode == 'A') {
                        localscope.parent.CallBack(0, 'Cancelled', localscope);
                    }
                });
            });
        }

        this.Options = [
            { 'id': 'ALL', 'name': 'ALL' },
            { 'id': 'CARD', 'name': 'CARD' },
            { 'id': 'UPI', 'name': 'UPI' }];
    }

    function paytmQR(parent) {
        this.parent = parent;
        this.status = '';
        this.timer = null;
        this.qrCodeId = '';
        this.UserID = '';
        this.GetCheckSum = function (key, data, callback) {
            this.parent.http({
                method: 'POST',
                url: "ConfigurationService/Checksum",
                data: { 'Item': { 'type': 'paytmqr', 'key': key, 'data': data } }
            }).success(function (result) {
                callback(result);
            }).error(function (result) {

            });
        };

        this.Run = function (settlement, qrimagecallback, qrresponsecallback) {
            this.parent.lock = true;
            this.cancelled = false;
            this.qrimagecallback = qrimagecallback;
            this.qrresponsecallback = qrresponsecallback;
            this.config = (this.parent.settings.int_payconfig &&
                this.parent.settings.qr_payconfig != null &&
                this.parent.settings.qr_payconfig != '') ? (JSON.parse(this.parent.settings.qr_payconfig)) : {};

            var billnos = [];
            var billids = [];
            if (this.parent.IsSingleWindow) {
                billnos.push(this.parent.CurrentSingleWindowSettlementNo);
                billids.push(this.parent.CurrentSingleWindowSettlementNo);
            }
            else {
                $.each(settlement.Billsettlement, function (k, v) {
                    billnos.push(v.billno);
                    billids.push(v.billno);
                });
            }
            this.UserID = this.parent.scope.UserInfo.id;
            this.OrderID = billids.join('_');
            this.Amount = Math.round(settlement.balanceamount);
            var paytmrequest = {
                "body": {
                    "mid": this.config.mid,
                    "orderId": this.OrderID,
                    "amount": Math.round(this.Amount).toFixed(2),
                    "businessType": "UPI_QR_CODE",
                    "posId": "Quanto-" + this.UserID
                },
                "head": {
                    "clientId": this.config.clientId,
                    "version": this.config.version,
                    "signature": ""
                }
            };


            var localscope = this;
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }

            this.GetCheckSum(this.config.accesskey, JSON.stringify(paytmrequest.body), function (checksumresult) {

                paytmrequest.head.signature = checksumresult.checksum;
                var request = {
                    'id': printerData.printerid,
                    'content': JSON.stringify(paytmrequest),
                    'mode': 'post',
                    'encoded': '',
                    'destination': localscope.config.sendurl
                };
                localscope.ManualSettlement = false;
                localscope.payrequest = JSON.stringify(request);
                localscope.parent.SendWebData({
                    method: 'POST',
                    url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/WebData',
                    data: localscope.payrequest
                }, function (result) {
                    if (result.body &&
                        result.body.resultInfo &&
                        result.body.resultInfo &&
                        result.body.resultInfo.resultStatus &&
                        result.body.resultInfo.resultStatus == 'SUCCESS') {

                        var history = {
                            'request': localscope.Request,
                            'response': result,
                            'name': localscope.Amount.toString() + "(" +
                                localscope.OrderID + ")",
                            'id': localscope.OrderID,
                            'refid': result.body.qrCodeId,
                            'TransactionNumber': localscope.OrderID,
                            'InvoiceNumber': localscope.OrderID,
                            'Amount': localscope.Amount,
                        };

                        var pinelabhistory = localStorageService.get('paytmqrlabhistory');
                        if (pinelabhistory == undefined || pinelabhistory == null)
                            pinelabhistory = [];
                        var exist = pinelabhistory.find(function (item) {
                            return item.TransactionNumber == history.TransactionNumber &&
                                item.InvoiceNumber == history.InvoiceNumber &&
                                item.Amount == history.Amount;
                        });
                        if (exist && exist != null) {
                            exist.request = history.request;
                            exist.response = history.response;
                            exist.name = history.name;
                            exist.id = history.id;
                            exist.refid = history.refid;
                        }
                        else {
                            pinelabhistory.push(history);
                        }
                        localStorageService.set('paytmqrlabhistory', pinelabhistory);
                        localscope.qrCodeId = result.body.qrCodeId;
                        localscope.qrimagecallback(result.body.image);
                        localscope.timeoutid = localscope.parent.timeout(function () {

                            localscope.GetCheckSum(localscope.config.accesskey, JSON.stringify({
                                "mid": localscope.config.mid,
                                "orderId": localscope.OrderID
                            }), function (checksumresult) {
                                localscope.CheckStatus(checksumresult.checksum, localscope, result);
                            });

                        }, 100);
                    }
                });
            });
        }



        this.ManualSettlement = false;
        this.ManualItemHistory = [];
        this.EnableManualSettlement = function () {
            this.ManualItemHistory = [];
            var data = localStorageService.get('paytmqrlabhistory');
            if (data && data != null) {
                this.ManualSettlement = true;
                this.ManualItemHistory = data;
            }
        }

        this.Cancel = function () {
            this.cancelled = true;
            if (this.timeoutid && this.timeoutid != null)
                this.parent.timeout.cancel(this.timeoutid);
        }

        this.Convert2Result = function (localscope, result) {
            var cresult = {};
            cresult.FullResponse = JSON.stringify(result);
            cresult.Request_Input = localscope.payrequest;
            cresult.paymentuniqueid = localscope.parent.CurrentSingleWindowSettlementNo;
            cresult.ResponseCode = result.body.resultInfo.resultStatus;
            cresult.ResponseMessage = result.body.resultInfo.resultMsg;
            cresult.ApprovalCode = result.body.txnId;
            cresult.RRN_No = result.body.bankTxnId;
            cresult.Amount = result.body.txnAmount;
            cresult.Txn_Date = result.body.txnDate;
            cresult.Txn_Type = result.body.txnType;
            cresult.BankInvoice_Num = result.body.bankTxnId;
            cresult.Batch_Number = result.body.orderId;
            cresult.Terminal_Id = result.body.posId;
            cresult.Merchant_Id = result.body.mid;
            cresult.PaymentMode = result.body.paymentMode.toUpperCase();
            return cresult;
        }

        this.CheckStatus = function (checksum, localscope, response) {
            var pinerequest = {
                "body": {
                    "mid": this.config.mid,
                    "orderId": this.OrderID
                },
                "head": {
                    "signature": checksum
                }
            };

            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }
            var request = {
                'id': printerData.printerid,
                'content': JSON.stringify(pinerequest),
                'mode': 'post',
                'encoded': '',
                'destination': localscope.config.statusurl
            };
            var srequest = JSON.stringify(request);
            localscope.parent.SendWebData({
                method: 'POST',
                url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/WebData',
                data: srequest
            }, function (result) {
                if (result.body && result.body.resultInfo) {
                    if (result.body.resultInfo.resultStatus == 'TXN_SUCCESS') {
                        result = localscope.Convert2Result(localscope, result);
                        localscope.qrresponsecallback(result);
                        localscope.parent.ClearSingleWindowNo();

                    }
                    else {
                        if (localscope.cancelled == false) {
                            localscope.timeoutid = localscope.parent.timeout(function () {
                                localscope.CheckStatus(checksum, localscope, response);
                            }, 100);
                        }
                    }
                }
                else {
                    localscope.qrCodeId = '';
                    localscope.parent.ClearSingleWindowNo();
                }
            });
        }
    }

    function razerQR(parent) {
        this.parent = parent;
        this.status = '';
        this.timer = null;
        this.txnId = '';
        this.UserID = '';

        this.Run = function (settlement, qrimagecallback, qrresponsecallback) {
            this.parent.lock = true;
            this.cancelled = false;
            this.qrimagecallback = qrimagecallback;
            this.qrresponsecallback = qrresponsecallback;
            this.config = (this.parent.settings.int_payconfig &&
                this.parent.settings.qr_payconfig != null &&
                this.parent.settings.qr_payconfig != '') ? (JSON.parse(this.parent.settings.qr_payconfig)) : {};

            var billnos = [];
            var billids = [];
            if (this.parent.IsSingleWindow) {
                billnos.push(this.parent.CurrentSingleWindowSettlementNo);
                billids.push(this.parent.CurrentSingleWindowSettlementNo);
            }
            else {
                $.each(settlement.Billsettlement, function (k, v) {
                    billnos.push(v.billno);
                    billids.push(v.billno);
                });
            }
            this.UserID = this.parent.scope.UserInfo.id;
            this.OrderID = billids.join('_');
            this.Amount = Math.round(settlement.balanceamount);

            var razerrequest = {
                "appKey": this.config.appKey,
                "username": this.config.username,
                "password": this.config.password,
                "amount": Math.round(this.Amount).toFixed(2),
                "externalRefNumber": this.OrderID,
                "qrCodeType": "BHARAT"
            };


            var localscope = this;
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }

            var request = {
                'id': printerData.printerid,
                'content': JSON.stringify(razerrequest),
                'mode': 'post',
                'encoded': '',
                'destination': localscope.config.sendurl
            };
            localscope.ManualSettlement = false;
            localscope.payrequest = JSON.stringify(request);
            localscope.parent.SendWebData({
                method: 'POST',
                url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/WebData',
                data: localscope.payrequest
            }, function (result) {
                if (result.success) {

                    var history = {
                        'request': localscope.Request,
                        'response': result,
                        'name': localscope.Amount.toString() + "(" +
                            localscope.OrderID + ")",
                        'id': localscope.OrderID,
                        'refid': result.txnId,
                        'TransactionNumber': localscope.OrderID,
                        'InvoiceNumber': localscope.OrderID,
                        'Amount': localscope.Amount,
                    };

                    var pinelabhistory = localStorageService.get('razerqrlabhistory');
                    if (pinelabhistory == undefined || pinelabhistory == null)
                        pinelabhistory = [];
                    var exist = pinelabhistory.find(function (item) {
                        return item.TransactionNumber == history.TransactionNumber &&
                            item.InvoiceNumber == history.InvoiceNumber &&
                            item.Amount == history.Amount;
                    });
                    if (exist && exist != null) {
                        exist.request = history.request;
                        exist.response = history.response;
                        exist.name = history.name;
                        exist.id = history.id;
                        exist.refid = history.refid;
                    }
                    else {
                        pinelabhistory.push(history);
                    }
                    localStorageService.set('razerqrlabhistory', pinelabhistory);
                    localscope.txnId = result.txnId;
                    localscope.qrimagecallback(result.qrCodeUri);
                    localscope.timeoutid = localscope.parent.timeout(function () {
                        localscope.CheckStatus(localscope, result);
                    }, 100);
                }
            });

        }



        this.ManualSettlement = false;
        this.ManualItemHistory = [];
        this.EnableManualSettlement = function () {
            this.ManualItemHistory = [];
            var data = localStorageService.get('razerqrlabhistory');
            if (data && data != null) {
                this.ManualSettlement = true;
                this.ManualItemHistory = data;
            }
        }

        this.Cancel = function () {
            this.cancelled = true;
            if (this.timeoutid && this.timeoutid != null)
                this.parent.timeout.cancel(this.timeoutid);
        }

        this.Convert2Result = function (localscope, result) {
            var cresult = {};
            cresult.FullResponse = JSON.stringify(result);
            cresult.Request_Input = localscope.payrequest;
            cresult.paymentuniqueid = localscope.parent.CurrentSingleWindowSettlementNo;
            cresult.ResponseCode = result.status;
            cresult.ResponseMessage = result.body.resultInfo.resultMsg;
            cresult.ApprovalCode = result.txnId;
            cresult.RRN_No = result.txnId;
            cresult.Amount = result.amount;
            cresult.Txn_Date = new Date(result.postingDate);
            cresult.Txn_Type = result.txnType;
            cresult.BankInvoice_Num = result.txnId;
            cresult.Batch_Number = result.externalRefNumber;
            cresult.Terminal_Id = result.tid;
            cresult.Merchant_Id = result.mid;
            cresult.PaymentMode = result.paymentMode.toUpperCase();
            if (cresult.PaymentMode == 'BHARATQR')
                cresult.PaymentMode = 'UPI';
            return cresult;
        }

        this.CheckStatus = function (localscope, response) {
            var razerrequest = {
                "username": localscope.config.username,
                "password": localscope.config.password,
                "txnId": localscope.txnId
            };

            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }
            var request = {
                'id': printerData.printerid,
                'content': JSON.stringify(pinerequest),
                'mode': 'post',
                'encoded': '',
                'destination': localscope.config.statusurl
            };
            var srequest = JSON.stringify(request);
            localscope.parent.SendWebData({
                method: 'POST',
                url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/WebData',
                data: srequest
            }, function (result) {
                if (result.body && result.body.resultInfo) {
                    if (result.body.resultInfo.resultStatus == 'TXN_SUCCESS') {
                        result = localscope.Convert2Result(localscope, result);
                        localscope.qrresponsecallback(result);
                        localscope.parent.ClearSingleWindowNo();

                    }
                    else {
                        if (localscope.cancelled == false) {
                            localscope.timeoutid = localscope.parent.timeout(function () {
                                localscope.CheckStatus(localscope, response);
                            }, 100);
                        }
                    }
                }
                else {
                    localscope.txnId = '';
                    localscope.parent.ClearSingleWindowNo();
                }
            });
        }
    }
}



