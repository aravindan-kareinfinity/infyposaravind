
var app = angular.module('InfinityLab');

app.controller('RechargeController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', 'restService', '$timeout', 'Excel', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, restService, $timeout, Excel, ngDialog) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.CanSave = true;
        $scope.Selected = '1';
        Number.prototype.round = function (places) {
            return +(Math.round(this + "e+" + places) + "e-" + places);
        }

        $scope.GetNumber = function (value) {
            value = parseFloat(value);
            if (typeof (value) == 'undefined' || value == null || isNaN(value))
                return 0;
            return value;
        };

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.TaxList = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.TaxList = results;
        });

        $scope.WarehouseLocations = restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
            $scope.WarehouseLocations = results;
        });

        $scope.StatusList = [{ 'id': -1, 'name': 'All' }, { 'id': 0, 'name': 'Pending' }, { 'id': 1, 'name': 'Replacement' },
        { 'id': 2, 'name': 'Credit Note' }, { 'id': 3, 'name': 'Debit Note' }, { 'id': 4, 'name': 'Return' },
        { 'id': 5, 'name': 'Alternate' }, { 'id': 6, 'name': 'Replaced Items' }];

        $scope.SwippingList = restService.Get('ReferencelistService/FULL-SWIPPINGMACINE', true).then(function (results) {
            $scope.SwippingList = results;
        });

        $scope.BankList = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            if (results.length == 0) {
                results.push({ 'id': -1, 'name': '-' });
            }
            $scope.BankList = results;
        });

        $scope.Schemes = restService.Get('ReferencelistService/RECHARGESCHEMES', true).then(function (results) {
            $scope.Schemes = results;
        });

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.Items = restService.Get('ReferencelistService/ITEMS', true).then(function (results) {
            $scope.Items = results;
        });

        $scope.CurrentLocation = 'Please Select';
        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
            results.push({ 'id': 0, 'name': 'WAREHOUSE', 'parentgroup': 0 });
            $scope.Recharge.locationid = 0;
            if (!typeof ($rootScope.LocationInfo.location.id) != 'undefined') {

                $timeout(function () {
                    $scope.CurrentLocation = $scope.SearchRef($rootScope.LocationInfo.location.id, $scope.Locations);
                    $scope.Recharge.locationid = $rootScope.LocationInfo.location.id;
                    $scope.Recharge.fromlocationid = $rootScope.LocationInfo.location.id;
                    $scope.Recharge.companyid = $rootScope.LocationInfo.company.id;
                }, 100);
            }
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });
        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });
        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.Permission = UserDetails.Info;
        $scope.SearchRef = restService.SearchReference;
        $scope.returnqty = 0;

        $scope.Entity = {
            "mode": 1,
            "productid": 0,
            "companyid": 0,
            "id": 0,
            "isactive": false,
            "createdby": 0,
            "modifiedby": 0,
            "version": 0,
            "ratereadonly": false,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            "itemid": 0,
            "qty": 0.0,
            "piece": 0.0,
            "barcode": null,
            "stockid": 0,
            "lrinvoiceitemsid": 0,
            "taxid": 0,
            "rate": 0.0,
            "tax": 0.0,
            "total": 0,
            "returnno": null,
            "Rechargedate": new Date(),
            "lrinvoiceid": 0,
            "lrid": 0,
            "discount": 0.0,
            "amount": 0.0,
            "lrinvoicereturnid": 0,
            "Items": [],
            "Lrinvoicereturngroups": [],
            "transportid": "0",
            "bundles": "0",
            "freightamt": "0.0",
            "freight": 0,
            "weight": "0.0",
            "remarks": ""
        };


        $scope.New = function () {
            $scope.LoadReturnList = [];
            $scope.Recharge = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.Recharge.Rechargedate = new Date();
        }
        $scope.New();



        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.Debitnote = false;

        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Debitnote = false;
            $scope.PrintDocument = { 'pages': [] };
            //$scope.PurchaseReturn = (JSON.parse(JSON.stringify($scope.Entity)));
        }


        $scope.SearchRequest = {
            Rechargedate: "",
            status: -1
        };

        $scope.SchemeRateChanged = function () {
            $scope.ShowCurrentBalance($scope.Recharge.locationid, $scope.Recharge.schemeitemid);

            var schemerate = $scope.SchemeItems.find(function (item) {
                return item.id == $scope.Recharge.schemeitemid;
            });
            if (typeof (schemerate) != 'undefined' && schemerate != null) {
                $scope.Recharge.rate = schemerate.rate;
                $scope.Recharge.cashback = schemerate.cashback;
                $scope.Recharge.discount = schemerate.discount;
                $scope.Recharge.ratereadonly = schemerate.rate > 0;
                $scope.Recharge.discountreadonly = schemerate.discount > 0;
            }
        }

        $scope.rechargeratechanged = function () {
            $scope.Recharge.cashback = (schemerate.rate * $scope.Recharge.cashbackinpercentage / 100).round(0);
            $scope.Recharge.paid_cash = schemerate.rate - schemerate.discount;
            $scope.Recharge.paid_card = 0;
        }

        $scope.SchemeChanged = function () {
            var scheme = $scope.Schemes.find(function (item) {
                return item.id == $scope.Recharge.schemeid;
            });

            if (typeof (scheme) != 'undefined' && scheme != null) {
                $scope.Recharge.scheme = scheme;
                $scope.Recharge.cashbackinpercentage = scheme.cashbackinpercentage;
                $scope.Recharge.productid = scheme.productid;
                $scope.Recharge.itemid = scheme.itemid;
                $scope.SchemeItems = scheme.items;
                if ($scope.Selected == '2')
                    $scope.RefereshCurrentBalance($scope.Recharge.fromlocationid, $scope.Recharge.schemeid)
                else
                    $scope.RefereshCurrentBalance($scope.Recharge.locationid, $scope.Recharge.schemeid)
            }
            else if ($scope.Selected == '2' && $scope.Recharge.fromlocationid>0)
            {
                $scope.RefereshCurrentBalance($scope.Recharge.fromlocationid, $scope.Recharge.schemeid)
            }
            else if ($scope.Recharge.locationid > 0) {
                $scope.RefereshCurrentBalance($scope.Recharge.locationid)
            }
        }

        $scope.SchemeItems = [];
        $scope.exporttoExcel = function (tableid, title) {
            $scope.exportHref = Excel.tableToExcel(tableid, title, 1, 'PURCHASE RETURN SUMMARY REPORT');
            $timeout(function () {
                var link = document.createElement('a');
                link.download = title + ".xls";
                link.href = $scope.exportHref;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }
        $scope.ErrorMessage = "";
        $scope.LoadReturnList = [];

        $scope.SearchCondition = {
            receivedbyemployeid: "",
            paidbyemployeeid: "",
            paidon: "",
            showSearchResult: false,
            searchtype: 0
        }

        $scope.SearchMode = '';
        $scope.Search = function (mode) {
            switch (mode) {
                case 'Main':
                case 'Transfer':
                case 'Recharge':
                    $scope.SearchMode = mode;
                    $scope.ShowSearch = true;
                    switch ($scope.SearchMode) {
                        case 'Main':
                            $scope.SearchRequest.tranmode = 1;
                            break;
                        case 'Transfer':
                            $scope.SearchRequest.tranmode = 2;
                            break;
                        case 'Recharge':
                            $scope.SearchRequest.tranmode = 3;
                            break;
                    }
                    break;
                case 'Search':
                    $scope.SearchRegister();
                    break;
            }
        }

        $scope.Save = function () {

            $scope.Recharge.tranmode = $scope.Selected;

            if (typeof ($scope.Recharge.schemeid) == 'undefined' || $scope.Recharge.schemeid == null) {
                $rootScope.Toaster.Info("Please Select Operator.!");
                return;
            }

            if (typeof ($scope.Recharge.companyid) == 'undefined' || $scope.Recharge.companyid == null) {
                $rootScope.Toaster.Info("Please Select Company.!");
                return;
            }

            if ($scope.Recharge.tranmode == '3') {
                if (typeof ($scope.Recharge.locationid) == 'undefined' || $scope.Recharge.locationid == null) {
                    $rootScope.Toaster.Info("Please Select Stock Location.!");
                    return;
                }

                if (typeof ($scope.Recharge.rate) == 'undefined' || $scope.Recharge.rate == null || $scope.Recharge.rate == '') {
                    $rootScope.Toaster.Info("Please enter the rate.!");
                    return;
                }

                var schemeitem = $scope.SchemeItems.find(function (item) {
                    return item.id == $scope.Recharge.schemeitemid;
                });

                if (typeof (schemeitem) == 'undefined' || schemeitem == null) {
                    $rootScope.Toaster.Info("Please Select Item.!")
                }

                $scope.Recharge.paid_card = $scope.GetNumber($scope.Recharge.paid_card);
                $scope.Recharge.paid_cash = $scope.GetNumber($scope.Recharge.paid_cash);
                $scope.Recharge.rate = $scope.GetNumber($scope.Recharge.rate);
                $scope.Recharge.discount = $scope.GetNumber($scope.Recharge.discount);
                if ($scope.Recharge.paid_card + $scope.Recharge.paid_cash != ($scope.Recharge.rate - $scope.Recharge.discount)) {
                    $rootScope.Toaster.Info("Please Enter the payment on respective input!");
                    return;
                }
                if ($scope.Recharge.paid_card > 0) {
                    var swipmachine = $scope.SwippingList.find(function (item) {
                        return item.id == $scope.Recharge.swipmachineid;
                    });
                    if (typeof (swipmachine) != 'undefined' && swipmachine != null) {
                        $scope.Recharge.swipbankid = swipmachine.additionalinfo.parentid;
                    }
                    else {
                        $rootScope.Toaster.Info("Please Select TID.!");
                        return;
                    }
                }

                $scope.Recharge.operatorname = $scope.SearchRef($scope.Recharge.schemeid, $scope.Schemes);
                $scope.Recharge.schemename = schemeitem.description;
                $scope.Recharge.amount = 0;

                if (schemeitem.rate == $scope.Recharge.rate) {
                    $scope.Recharge.cashback = schemeitem.cashback;
                }
                else if ($scope.Recharge.rate > 0 && $scope.Recharge.cashbackinpercentage > 0) {
                    $scope.Recharge.cashback = ($scope.GetNumber($scope.Recharge.rate) * $scope.Recharge.cashbackinpercentage / 100).round(2);
                }
            }

            if ($scope.Recharge.tranmode == '1') {
                if (typeof ($scope.Recharge.locationid) == 'undefined' || $scope.Recharge.locationid == null) {
                    $rootScope.Toaster.Info("Please Select Location.!");
                    return;
                }

                if (typeof ($scope.Recharge.amount) == 'undefined' || $scope.Recharge.amount == null || $scope.Recharge.amount == '') {
                    $rootScope.Toaster.Info("Please enter the amount.!");
                    return;
                }
                $scope.Recharge.schemename = $scope.SearchRef($scope.Recharge.schemeid, $scope.Schemes);
                $scope.Recharge.rate = 0;
                $scope.Recharge.cashback = 0;
                $scope.Recharge.discount = 0;
            }

            if ($scope.Recharge.tranmode == '2') {
                if (typeof ($scope.Recharge.fromlocationid) == 'undefined' || $scope.Recharge.fromlocationid == null) {
                    $rootScope.Toaster.Info("Please Select from Location.!");
                    return;
                }

                if (typeof ($scope.Recharge.tolocationid) == 'undefined' || $scope.Recharge.tolocationid == null) {
                    $rootScope.Toaster.Info("Please Select to Location.!");
                    return;
                }

                if (typeof ($scope.Recharge.amount) == 'undefined' || $scope.Recharge.amount == null || $scope.Recharge.amount == '') {
                    $rootScope.Toaster.Info("Please enter the amount.!");
                    return;
                }
                $scope.Recharge.schemename = $scope.SearchRef($scope.Recharge.schemeid, $scope.Schemes);
                $scope.Recharge.rate = 0;
                $scope.Recharge.cashback = 0;
                $scope.Recharge.discount = 0;
                $scope.Recharge.fromlocation = $scope.SearchRef($scope.Recharge.fromlocationid, $scope.Locations);
                $scope.Recharge.tolocation = $scope.SearchRef($scope.Recharge.tolocationid, $scope.Locations);
            }

            if ($scope.Recharge.Rechargedate == null || $scope.Recharge.Rechargedate == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Info("Please Select Date.!");
                return;
            }

            var request = {
                "Item": (JSON.parse(JSON.stringify($scope.Recharge))),
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RechargeService/Save",
                data: srequest
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Recharge Saved Successfully");
                    if ($scope.Recharge.tranmode == '1') {
                        $scope.Recharge.supplierid = 0;
                        $scope.Recharge.invoiceno = '';
                        $scope.Recharge.taxid = 0;
                        $scope.Recharge.isinterstatesale = false;
                        $scope.Recharge.amount = 0;
                        $scope.Recharge.billvalue = 0;
                        $scope.ShowCurrentBalance($scope.Recharge.locationid, $scope.Recharge.schemeid, function (result) {
                            $scope.CurrentBalanceList = result;
                        });
                    } else if ($scope.Recharge.tranmode == '2') {
                        $scope.Recharge.tolocationid = 0;
                        $scope.Recharge.amount = 0;
                        $scope.ShowCurrentBalance($scope.Recharge.fromlocationid, $scope.Recharge.schemeid, function (result) {
                            $scope.CurrentBalanceList = result;
                        });
                    }
                    else {
                        $scope.Recharge.schemeitemid = 0;
                        $scope.Recharge.rate = 0;
                        $scope.Recharge.discount = 0;
                        $scope.Recharge.paid_cash = 0;
                        $scope.Recharge.paid_card = 0;
                        $scope.Recharge.swipmachineid = 0;
                        $scope.Recharge.customerphoneno = '';
                        $scope.Recharge.customername = '';
                        $scope.TodayRecharge.push(result);
                    }
                }
                else if (typeof (result.errormessage) != 'undefined') {
                    $rootScope.Toaster.Error(result.errormessage);
                }
                else {
                    $rootScope.Toaster.Error("Can't Save, check the data and save again");
                }
            }).error(function (result) {
            });

        }


        $scope.SearchRegister = function () {
            request = {
                "Item": $scope.SearchRequest,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RechargeService/Search",
                data: srequest
            }).success(function (result) {
                if ($scope.igrid != null)
                    $scope.igrid.destroy();
                $scope.Register = result;
                $scope.ConfigGrid();
                $scope.igrid.UpdateData(result);
            }).error(function (result) {
            });
        }

        $scope.igrid = null;
        $scope.ConfigGrid = function () {
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 50);
            igrid.options.showHeaderRow = true;
            igrid.AddFormatedColumn('rechargedate', 'Date', 80, 'dd-MM-yyyy');
            switch ($scope.SearchRequest.tranmode) {
                case 1:
                    igrid.AddReferenceColumn('locationid', 'Location', 200, $scope.Locations);
                    igrid.AddReferenceColumn('receivedemployeeid', 'Employee', 200, $scope.Employees);
                    igrid.AddSimpleColumn('schemename', 'Operator', 200);
                    igrid.AddFormatedColumn('amount', 'Amount', 50);
                    var commands = "<input type='button' class='btn btn-primary btn-xs' value='Edit'/>"
                    commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"
                    igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                        $scope.$apply(function () {
                            if ($(e.target).val() == 'Delete') {

                                var request = {
                                    "Item": args.item,
                                    "SecurityToken": "String content"
                                };

                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "RechargeService/Delete",
                                    data: srequest
                                }).success(function (result) {
                                    $scope.Register.splice($scope.Register.indexOf(args.item), 1);
                                    $scope.igrid.UpdateData($scope.Register);
                                }).error(function (result) {

                                });

                            }
                        });
                    });
                    break;
                case 2:
                    igrid.AddReferenceColumn('fromlocationid', 'From', 200, $scope.Locations);
                    igrid.AddReferenceColumn('tolocationid', 'To', 200, $scope.Locations);
                    igrid.AddReferenceColumn('receivedemployeeid', 'Employee', 200, $scope.Employees);
                    igrid.AddSimpleColumn('schemename', 'Operator', 200);
                    igrid.AddFormatedColumn('amount', 'Amount', 50);
                    break;
                case 3:
                    igrid.AddReferenceColumn('locationid', 'Location', 200, $scope.Locations);
                    igrid.AddReferenceColumn('receivedemployeeid', 'Employee', 200, $scope.Employees);
                    igrid.AddSimpleColumn('schemename', 'Operator', 200);
                    igrid.AddFormatedColumn('rate', 'Rate', 50);
                    igrid.AddSimpleColumn('customername', 'Customer', 200);
                    igrid.AddSimpleColumn('customerphoneno', 'Mobile No', 200);
                    break;
            }
            igrid.Initialize();
            $scope.igrid = igrid;
        }

        $scope.SearchCustomer = function () {

            var dialog = ngDialog.open({
                template: 'SearchCustomer',
                data: [$scope],
                width: '800px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    $scope.PopupList = [];
                    $scope.Customer = null;
                    $scope.CloseIfOne = false;

                    $scope.Select = function () {
                        $scope.Customer = null;
                        $.each($scope.PopupList, function (k, v) {
                            if (v.selected)
                                $scope.Customer = v;
                        });
                        if ($scope.Customer != null)
                            $scope.closeThisDialog($scope.Customer);
                    };

                    $scope.Create = function () {
                        var request = {
                            "Item": {
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno,
                                'gstno': $scope.gstno, 'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno
                            },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Save",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(result);
                        });
                    };

                    $scope.hasvalue = function (val) {
                        return typeof (val) != 'undefined' && val != null &&
                            val != '' && val.length > 0;
                    }

                    $scope.Search = function (closeIfOne) {

                        $scope.CloseIfOne = typeof (closeIfOne) != 'undefined' ? closeIfOne : false;
                        if (!$scope.hasvalue($scope.name) &&
                            !$scope.hasvalue($scope.area) &&
                            !$scope.hasvalue($scope.contactno) &&
                            !$scope.hasvalue($scope.gstno) &&
                            !$scope.hasvalue($scope.loyeltyno))
                            return;

                        var request = {
                            "Item": {
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno, 'gstno': $scope.gstno,
                                'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno
                            },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Search",
                            data: srequest
                        }).success(function (result) {
                            if (result.length == 1 && $scope.CloseIfOne) {
                                $scope.closeThisDialog(result[0]);
                            }
                            $scope.PopupList = result;
                        });
                    };

                    $timeout(function () {
                        $scope.setFocuscard_no();
                    }, 10);

                    $scope.contactno = $scope.ngDialogData[0].Recharge.customerphoneno;
                    if (typeof ($scope.contactno) != 'undefined' && $scope.contactno != null) {
                        if ($scope.contactno.length != 10 || $scope.ngDialogData[0].GetNumber($scope.contactno) <= 0) {
                            $scope.name = $scope.contactno;
                            $scope.contactno = '';
                        }
                        $scope.Search(true);
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape' && data.value.id > 0) {
                        $scope.Recharge.customerid = data.value.id;
                        $scope.Recharge.customername = data.value.name;
                        $scope.Recharge.customerphoneno = data.value.mobileno;
                        $scope.Recharge.customeraddress = data.value.address;
                    }
                }
            });
        }

        $scope.CurrentBalanceList = [];
        $scope.TodayRecharge = [];

        $scope.RefereshCurrentBalance = function (locationid, schemeid) {
            $scope.ShowCurrentBalance(locationid, schemeid);
        }

        $scope.ShowCurrentBalance = function (locationid, schemeid,callback) {
            var request = {
                "Item": {
                    'locationid': locationid,
                    'schemeid': schemeid
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "RechargebalanceService/Search",
                data: srequest
            }).success(function (result) {
                $scope.ErrorMessage = '';
                $scope.CurrentBalanceList = result;
                if (typeof (callback) != 'undefined') {
                    callback(result);
                }
                else if (result.length == 1 && typeof (schemeid) != 'undefined') {
                    $scope.ErrorMessage = 'Balance - ' + result[0].balance;
                }
            }).error(function (result) {

            });
        }

        $timeout(function () {

            if (!typeof ($rootScope.LocationInfo.location.id) != 'undefined') {
                $scope.CurrentLocation = $scope.SearchRef($rootScope.LocationInfo.location.id, $scope.Locations);
                $scope.ShowCurrentBalance($rootScope.LocationInfo.location.id);
            }

        }, 10);

    }]);

