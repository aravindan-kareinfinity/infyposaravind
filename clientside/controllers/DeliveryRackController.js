var app = angular.module('InfinityLab');

app.controller('DeliveryRackController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter', 'ngDialog', 'localStorageService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter, ngDialog, localStorageService) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.HistoryMode = false;
        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.RefereshList = [{ 'id': 0, 'name': 'None' },
        { 'id': '15000', 'name': '15 Seconds' },
        { 'id': '30000', 'name': '30 Seconds' },
        { 'id': '60000', 'name': '1 Minutes' }];

        $scope.Entity = {
            id: 0,
            companyid: 0,
            locationid: 0,
            customerid: 0,
            sourcetype: "",
            employeeid: "",
            sourceid: "",
            bankid: "",
            chequedate: "",
            chequeno: "",
            amount: "",
            accountname: "",
            createdon: "",
            modifiedon: "",
            status: "",
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1
        }
        $scope.ChequeBox = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.ChequeBox.sourcetype = "Direct";

        $scope.SearchResult = [];
        $scope.totalamount = 0;
        $scope.paidamt = 0;

        $scope.ErrorStyle = "success";
        $scope.ErrorMessage = "";
        $scope.InactiveRacks = false;

        $scope.SearchCondition = {
            accountname: "",
            fromdate: "",
            todate: "",
            status: ""
        }

        $scope.Save = function () {
            var ismodify = $scope.ChequeBox.id && $scope.ChequeBox.id > 0;
            request = {
                "Item": $scope.ChequeBox,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BankchequetransactionService/Save",
                data: srequest
            }).success(function (result) {
                $scope.selectedcount = 0;
                if (ismodify) {
                    $scope.igrid.UpdateData($scope.SearchResult);
                }
                else {
                    $scope.SearchResult.push(result);
                    $scope.igrid.UpdateData($scope.SearchResult);
                }
                $rootScope.Toaster.Info("Cheque Details Saved Successfully.!");
                $scope.ChequeBox = (JSON.parse(JSON.stringify($scope.Entity)));

            }).error(function (result) {
            });

        }

        $scope.SearchList = function (inactive) {
            $scope.InactiveRacks = inactive;
            request = {
                "Item": {
                    'ShowInactive': inactive,
                    'locationid': $scope.LocationID
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DeliveryrackService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.ListConfig();
                $scope.setFocusbillno();
            }).error(function (result) {
            });
        }

        $scope.CheckRack = function (billnoorcardno) {
            if ($scope.SupportWalkin) {
                var request = {
                    "Item": { 'billno': billno },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "DeliveryrackService/CheckWalkin",
                    data: srequest
                }).success(function (results) {
                    if (results.newcard) {

                    }
                }).error(function (result) {
                });

            }
            else {
                $scope.RelaseRack(billnoorcardno);
            }
        }


        $scope.RelaseRack = function (billno) {
            var request = {
                "Item": { 'billno': billno },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "DeliveryrackService/SetRackFree",
                data: srequest
            }).success(function (results) {
                if (results && results != null && results.length > 0) {
                    var exist = $scope.SearchResult.find(function (item) {
                        return item.id == results[0].id;
                    });
                    if (exist && exist != null) {
                        $scope.ReleaseCounter = exist.code;
                        $scope.ReleaseCounterMissmatch = false;
                        if (results[0].addinfo.noofpieces != results[0].pieces) {
                            $scope.ReleaseCounterMissmatch = true;
                            $scope.ReleaseCounter = results[0].code + "(" + results[0].pieces + '/' + results[0].addinfo.noofpieces + ")";
                        }
                        exist.mobileno = '';
                        exist.billno = '';
                        exist.customer = '';
                        exist.card = '';
                        exist.addinfo = null;
                        $scope.selectedcount = 0;
                        $scope.igrid.UpdateData($scope.SearchResult);
                        $scope.billno = '';
                        $rootScope.Toaster.Info("Rack - " + exist.code);
                    }
                    else {
                        $rootScope.Toaster.Error("Bill Not avilable in the rack");
                    }
                }
                else {
                    $rootScope.Toaster.Error("Bill Not avilable in the rack");
                }
            }).error(function (result) {
            });
        }
        $scope.ReleaseCounterMissmatch = false;
        $scope.ReleaseCounter = '';
        $scope.Timer = null;
        $scope.RefreshChanged = function () {
            if ($scope.refereshid > 0) {
                $scope.RefreshList();
            }
        }

        $scope.RefreshList = function () {
            if ($scope.Timer != null) {
                $timeout.cancel($scope.Timer);
            }
            $scope.SearchList($scope.InactiveRacks);
            $scope.Timer = $timeout(function () {
                $scope.RefreshList();
            }, parseInt($scope.refereshid));
        }

        $scope.Timer = null;
        $scope.$on("$destroy", function () {
            if ($scope.Timer != null)
                $timeout.cancel($scope.Timer);
        });


        $scope.ListConfig = function () {
            var seletedRows = [];
            if ($scope.igrid != null) {
                seletedRows = this.igrid.GetSelectedRows();
                $scope.igrid.destroy();
            }

            var igrid = new InfyGrid('gridRackContainer', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('code', 'Rack', 150);
            igrid.AddSimpleColumn('card', 'Card', 100);
            igrid.AddSimpleColumn('mobileno', 'Mobile No', 150);
            igrid.AddSimpleColumn('customer', 'Customer', 150);
            igrid.AddSimpleColumn('billno', 'Bill No', 150);

            var column = igrid.AddCustomColumn('InTime', 'In Time', 180, function (item, col) {
                if (item.addinfo && item.addinfo != null) {
                    return $filter('date')(new Date(item.addinfo.starton), 'dd-MM-yyyy hh:mm a')
                }
                return '';
            });

            var column = igrid.AddCustomColumn('Duration', 'Duration', 110, function (item, col) {
                if (item.addinfo && item.addinfo != null) {
                    return '' + ((new Date() - new Date(item.addinfo.starton)) / 60000).round(0) + ' sec';
                }
                return '';
            });

            var column = igrid.AddCustomColumn('Noofbills', 'No Of Bills', 110, function (item, col) {
                if (item.addinfo && item.addinfo != null) {
                    return item.addinfo.noofbills;
                }
                return '';
            });

            var column = igrid.AddCustomColumn('noofpieces', 'No Of Pieces', 110, function (item, col) {
                if (item.addinfo && item.addinfo != null) {
                    if (item.pieces > 0 && item.addinfo.noofpieces != item.pieces)
                        return '(' + item.pieces + '/' + item.addinfo.noofpieces + ')';
                    return item.addinfo.noofpieces;
                }
                return '';
            });

            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };

            igrid.onDblClick = function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Noofbills') {
                    if (item.addinfo.noofbills &&
                        item.addinfo.noofbills > 1) {
                        $timeout(function () {
                            $scope.RemoveEstimate(item);
                        },10)
                    }
                }
            };

            igrid.EnableTotal();
            igrid.Initialize();
            igrid.setCellCSS('igridbigrows');
            igrid.dataView.getItemMetadata = function (row) {
                var rowdata = igrid.grid.getData(
                ).getItem(row);

                if (rowdata.addinfo == undefined || rowdata.addinfo == null) {
                    rowdata.style = '';
                    return { 'cssClasses': '' };
                }
                if (rowdata.billno == undefined || rowdata.billno == null || rowdata.billno == '') {
                    rowdata.style = 'igridbigrowswithmark';
                    return { 'cssClasses': '' };
                }

                if (rowdata.pieces > 0 && rowdata.addinfo.noofpieces != rowdata.pieces) {
                    rowdata.style = 'igridbigrowswithred';
                    return { 'cssClasses': '' };
                }

                rowdata.style = 'igridbigrowswithgreen';
                return { 'cssClasses': '' };
            };
            $scope.igrid = igrid;
            $scope.selectedcount = 0;
            $scope.igrid.UpdateData($scope.SearchResult);

            if (seletedRows.length > 0) {
                var selectedrows = [];
                $.each($scope.SearchResult, function (k, v) {
                    var exist = seletedRows.find(function (item) {
                        return item.id == v.id;
                    });
                    if (exist && exist != null)
                        selectedrows.push(k);
                })
                $scope.igrid.SetSelectedRows(selectedrows);
            }
        }
        $scope.selectedcount = 0;
        $scope.HasLocationSelected = false;
        $scope.AllowDelete = false;

        $scope.RemoveEstimate = function (item) {

            var dialog = ngDialog.open({
                template: 'EstimateBills',
                data: {
                    'scope': $scope,
                    'item': item
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.item = $scope.ngDialogData.item;
                    $scope.bills = [];
                    $.each($scope.item.addinfo.billnos, function (k, v) {
                        $scope.bills.push({ 'name': v, 'id': $scope.item.addinfo.billids[k] });
                    });
                    $scope.Save = function () {
                        if ($scope.billid && $scope.billid != null && $scope.billid != '') {
                            var bill = $scope.bills.find(function (item) { return item.id == $scope.billid; });
                            $scope.closeThisDialog(bill);
                        }
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        var request = {
                            "Item": { 'id': item.id, 'billid': data.value.id, 'billno': data.value.name },
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "DeliveryrackService/RemoveEstimate",
                            data: srequest
                        }).success(function (result) {
                            if (result && result.id > 0) {
                                var ex = $scope.SearchResult.find(function (item) {
                                    return item.id == result.id;
                                });
                                ex.addinfo = result.addinfo;
                                $scope.selectedcount = 0;
                                $scope.igrid.UpdateData($scope.SearchResult);
                            }
                        }).error(function (result) {

                        });

                    }
                }
            });

        }

        $scope.UpdateStatus = function (disabled, hasdelete, hasclear) {
            var seletedRows = this.igrid.GetSelectedRows();
            var ids = [];
            $.each(seletedRows, function (k, v) {
                ids.push(v.id);
            })

            var request = {
                "Item": { 'ids': ids, 'disabled': disabled, 'hasclear': hasclear, 'hasdelete': hasdelete },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "DeliveryrackService/UpdateStatus",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult.push(result);
                $scope.selectedcount = 0;
                $scope.igrid.UpdateData($scope.SearchResult);
            }).error(function (result) {

            });
        }


        $scope.EditRack = function () {
            var seletedRows = this.igrid.GetSelectedRows();
            restService.Get('DeliveryrackService/Find/' + seletedRows[0].id, true).then(function (results) {
                $scope.AddRack(results);
            });
        }

        $scope.AddRack = function (rackdetail) {

            if (rackdetail == undefined || rackdetail == null)
                rackdetail = {};

            var dialog = ngDialog.open({
                template: 'AddRack',
                data: {
                    'scope': $scope,
                    'rack': rackdetail
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Locations = $scope.ngDialogData.scope.Locations;
                    $scope.rack = $scope.ngDialogData.rack;
                    $scope.Save = function () {
                        $scope.closeThisDialog($scope.rack);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        var request = {
                            "Item": data.value,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "DeliveryrackService/Save",
                            data: srequest
                        }).success(function (result) {
                            var ex = $scope.SearchResult.find(function (item) {
                                return item.id == result.id;
                            });
                            if (ex && ex != null) {
                                $scope.SearchResult.splice($scope.SearchResult.indexOf(ex), 1);
                            }
                            $scope.SearchResult.push(result);
                            $scope.selectedcount = 0;
                            $scope.igrid.UpdateData($scope.SearchResult);
                        }).error(function (result) {

                        });

                    }
                }
            });
        }

        $scope.Margeto = function (supportmultiplescan) {

            var seletedRows = $scope.igrid.GetSelectedRows();

            var dialog = ngDialog.open({
                template: 'Margeto',
                data: {
                    'scope': $scope,
                    'rows': seletedRows,
                    'multi': supportmultiplescan
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parentscope = $scope.ngDialogData.scope;
                    $scope.rows = $scope.ngDialogData.rows;
                    $scope.multi = $scope.ngDialogData.multi;
                    $scope.AddRackCode = function () {
                        var rack = $scope.parentscope.SearchResult.find(function (item) {
                            return item.code == $scope.rackcode || item.card == $scope.rackcode;
                        });
                        if (rack && rack != null && $scope.rows.indexOf(rack) < 0) {
                            rack.name = rack.code;
                            if (rack.card && rack.card != null && rack.card != '')
                                rack.name = rack.code + '(' + rack.card + ')';
                            $scope.rows.push(rack);
                        }
                        else {
                            $scope.rackcode = '';
                            return;
                        }
                        var rows = $scope.rows;
                        $scope.rows = null;
                        $scope.rackcode = '';
                        $timeout(function () {
                            $scope.rows = rows;
                        },50)
                    }
                    $.each($scope.rows, function (k, v) {
                        v.name = v.code;
                        if (v.card && v.card != null && v.card != '')
                            v.name = v.code + '(' + v.card + ')';
                    })
                    $scope.Save = function () {
                        if ($scope.rackid && $scope.rackid != null && $scope.rackid>0)
                            $scope.closeThisDialog($scope.rackid);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        var request = {
                            "Item": {
                                'primaryid': data.value,
                                'secondary': [],
                                'locationid': $scope.LocationID
                            },
                            "securitytoken": "String content"
                        }

                        $.each(seletedRows, function (k, v) {
                            request.Item.secondary.push(v.id);
                        });

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "DeliveryrackService/Merge",
                            data: srequest
                        }).success(function (result) {
                            $scope.SearchResult = result;
                            $scope.ListConfig();
                            $scope.setFocusbillno();
                        }).error(function (result) {

                        });
                    }
                }
            });
        }

        $scope.LocationID = 0;
        $scope.SupportWalkin = false;
        $scope.ApplicationSettings = function () {

            if (typeof ($rootScope.UserInfo) == 'undefined' ||
                typeof ($rootScope.UserInfo.appsettings) == 'undefined' ||
                $rootScope.UserInfo.appsettings == null) {
                $timeout(function () {
                    $scope.ApplicationSettings();
                }, 10);
                return;
            }

            $scope.SupportWalkin = false || $rootScope.UserInfo.appsettings.system_supportdeliverywalk
            $scope.AllowDelete = $rootScope.UserInfo.p_DELIVERYRACK &&
                $rootScope.UserInfo.p_DELIVERYRACK.delete;

            if (!$scope.HasLocationSelected) {
                $rootScope.Triggers.SelectLocation('', function () {
                    $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' &&
                        typeof ($rootScope.LocationInfo.location) != 'undefined');
                    if ($scope.HasLocationSelected) {
                        $scope.LocationName = $scope.SearchRef($rootScope.LocationInfo.location.id, $scope.Locations);
                        $scope.LocationID = $rootScope.LocationInfo.location.id;
                        $scope.SearchList(false);
                    }
                });
            }

            $scope.refereshid = 60000;
            $scope.RefreshChanged();
        }

        $rootScope.OnLocationInfoReady(function () {
            $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' &&
                typeof ($rootScope.LocationInfo.location) != 'undefined');
            if ($scope.HasLocationSelected) {
                $scope.LocationName = $scope.SearchRef($rootScope.LocationInfo.location.id, $scope.Locations);
                $scope.LocationID = $rootScope.LocationInfo.location.id;
                $scope.SearchList(false);
            }
        });

        $scope.HistoryRack = null;
        $scope.ShowHistory = function (status) {
            if (status) {
                if ($scope.Timer != null) {
                    $timeout.cancel($scope.Timer);
                }
                $scope.HistoryMode = true;
                $scope.HistoryRack = this.igrid.GetSelectedRows()[0];
            }
            else {
                $scope.HistoryMode = false;
                $scope.RefreshList();
            }
        }

        $scope.SearchHistory = function () {
            var request = {
                "Item": {
                    'deliveryrackid': $scope.HistoryRack.id,
                    'deliveredon': $scope.historydate
                },
                "securitytoken": "String content"
            }

            $http({
                method: 'POST',
                url: "DeliveryrackService/History",
                data: request
            }).success(function (result) {
                if ($scope.igrid != null) {
                    $scope.igrid.destroy();
                }
                var igrid = new InfyGrid('gridRackContainer', '400px', '100%', true, true, true, 30);
                igrid.options.showHeaderRow = true;
                igrid.AddSimpleColumn('contactno', 'Mobile No', 150);
                igrid.AddSimpleColumn('customername', 'Customer', 150);
                igrid.AddSimpleColumn('billno', 'Bill No', 150);
                igrid.AddSimpleColumn('amount', 'Value', 150);
                igrid.AddSimpleColumn('qty', 'Pieces', 150);
                var column = igrid.AddCustomColumn('receivedon', 'In Time', 180, function (item, col) {
                    return $filter('date')(new Date(item.receivedon), 'dd-MM-yyyy hh:mm a')
                });
                var column = igrid.AddCustomColumn('deliveredon', 'Out Time', 180, function (item, col) {
                    return $filter('date')(new Date(item.deliveredon), 'dd-MM-yyyy hh:mm a')
                });
                igrid.AddSimpleColumn('deliveredby', 'Delivered By', 150);

                igrid.EnableTotal();
                igrid.Initialize();
                $scope.igrid = igrid;
                $scope.selectedcount = 0;
                $scope.igrid.UpdateData(result);

            }).error(function (result) {

            });
        }

        $timeout(function () {
            $scope.ApplicationSettings();
        }, 100);

    }]);

