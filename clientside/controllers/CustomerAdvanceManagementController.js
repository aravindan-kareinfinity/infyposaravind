
var app = angular.module('InfinityLab');

app.controller('CustomerOrderontroller', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter', 'ngDialog', 'localStorageService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter, ngDialog, localStorageService) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.DateToday = new Date();
        $scope.Navigation.SetMode('SEARCH');
        $scope.StatusList = [{ 'id': 0, 'name': 'New' }, { 'id': 1, 'name': 'Progress' },
        { 'id': 2, 'name': 'Completed' }, { 'id': 3, 'name': 'Cancelled' }, { 'id': -9, 'name': 'All' }];

        $scope.PaymentModes = [{ 'id': 1, 'name': 'Inter Bank Transfer' },
        { 'id': 2, 'name': 'Cheque/DD' },
        { 'id': 3, 'name': 'Cash' },
        { 'id': 4, 'name': 'Card' },
        { 'id': 5, 'name': 'UPI' }];



        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.BrandName = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandName = results;
        });
        $scope.CustomerCategory = restService.Get('ReferencelistService/CUSTOMERCATEGORY', true).then(function (results) {
            $scope.CustomerCategory = results;
        });

        $scope.Colors = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Colors = results;
        });

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.Cities = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.Cities = results;
        });

        $scope.States = restService.Get('ReferencelistService/STATE', true).then(function (results) {
            $scope.States = results;
        });

        $scope.Patterns = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Patterns = results;
        });

        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
        });

        $scope.Material = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Material = results;
        });

        $scope.Styles = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Styles = results;
        });


        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });

        $scope.TypeList = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.TypeList = results;
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Location = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.Banks = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.Banks = results;
        });
        $scope.SearchRequest = {
            status: 0
        }

        restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.AllCounters = results;
        });

        $scope.FilterCounter = function (locationid) {
            var lcounters = [];
            $.each($scope.AllCounters, function (k, v) {
                if (v.locationid == locationid) {
                    lcounters.push(v);
                }
            })
            $scope.Counters = lcounters;
        }

        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.ShowLocationInfo = function () {

            if ($rootScope == undefined || $rootScope.UserInfo == undefined || $rootScope.UserInfo.appsettings == undefined) {
                $timeout(function () {
                    $scope.ShowLocationInfo();
                }, 50);
                return;
            }

            $scope.HasLocationSelected = $rootScope.LocationInfo && $rootScope.LocationInfo.location;
            $scope.HasCounterSelected = $rootScope.LocationInfo && $rootScope.LocationInfo.counter;
            $scope.FinanceEnabled = $rootScope.UserInfo.appsettings.int_finance;
            if ($scope.FinanceEnabled && $scope.Ledgers == null) {
                $scope.Ledgers = restService.Get('ReferencelistService/LEDGERS/6_7_11_10_5', true).then(function (results) {
                    $scope.Ledgers = results;
                });
            }

            if ($scope.HasCounterSelected) {
                $scope.counterid = $rootScope.LocationInfo.counter.id;
                $scope.CounterName = $scope.SearchRef($rootScope.LocationInfo.counter.id, $scope.Counters);
            }
            else {
                $scope.CounterName = 'SELECT COUNTER';
            }

            if ($scope.HasLocationSelected) {
                $scope.locationid = $rootScope.LocationInfo.location.id;
                $scope.LocationName = $scope.SearchRef($rootScope.LocationInfo.location.id, $scope.Locations);
            }
            else {
                $scope.LocationName = 'SELECT LOCATION';
            }
        }

        $scope.LedgerChanged = function () {
            var ledger = $scope.Ledgers.find(function (item) {
                return item.id == $scope.paymentitem.ledgerid;
            });
            if (ledger && ledger != null) {
                $scope.paymentitem.bankid = ledger.bankid;
            }
        }

        $timeout(function () {
            $scope.ShowLocationInfo();
        }, 50);


        $scope.Entity = {
            id: 0,
            companyid: 0,
            locationid: 0,
            customerid: 0,
            modeofdelivery: 0,
            orderdate: new Date(),
            planneddeliverydate: new Date(),
            deliverydate: new Date(),
            orderamount: 0,
            paidamount: 0,
            customerorderno: "",
            notes: "",
            code: "",
            contactperson: "",
            contactno: "",
            isactive: true,
            createdby: 0,
            modifiedby: 0,
            version: 0,
            createdon: new Date(),
            modifiedon: new Date(),
            deliveryinfo: "",
            orderitems: { "items": [] },
            statushistory: "",
            communication: { "items": [] },
            paymentinfo: { "items": [] },
            vendorinfo: "",
            name: "",
            address: "",
            phoneno: "",
            area: 0,
            status: 0,
            totalqty: ""

        }
        $scope.ShowSearch = false;

        $scope.New = function () {
            $scope.CustomerOrder = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.CustomerOrder.orderitems = [];
            $scope.CustomerOrder.paymentinfo = [];
            $scope.CustomerOrder.communication = [];

            $scope.ProductDetails = [];
            $scope.PaymentDetails = [];
            $scope.SearchResult = [];
            $scope.totalamount = 0;
            $scope.orderqty = 0;

            $scope.paidamt = 0;

            $scope.ErrorStyle = "success";
            $scope.ErrorMessage = "";
            $scope.ShowSearch = true;
        }
        $scope.New();
        $scope.ShowSearch = false;
        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }
        $scope.GetNumber = function (value) {
            if (typeof (value) == 'number')
                return value;

            if (typeof (value) == 'undefined' || value == '')
                return 0

            nvalue = parseFloat(value);
            if (isNaN(nvalue))
                return 0;

            return nvalue;
        };
        $scope.ManageOrderItem = function (action, value) {
            if (action == 'delete') {
                $scope.CustomerOrder.orderitems.splice($scope.CustomerOrder.orderitems.indexOf(value), 1);
            }
            else if (action == 'edit') {
                $scope.CustomerOrder.supplierid = value.supplierid;
                $scope.CustomerOrder.productid = value.productid;
                $scope.CustomerOrder.brandid = value.brandid;
                $scope.CustomerOrder.styleid = value.styleid;
                $scope.CustomerOrder.sizeid = value.sizeid;
                $scope.CustomerOrder.colorid = value.colorid;
                $scope.CustomerOrder.designno = value.designno;
                $scope.CustomerOrder.qty = value.qty;
                $scope.CustomerOrder.price = value.price;

                $scope.CustomerOrder.orderitems.splice($scope.CustomerOrder.orderitems.indexOf(value), 1);
            }

            $scope.totalamount = 0;
            $scope.orderqty = 0;
            $.each($scope.CustomerOrder.orderitems, function (k, v) {
                $scope.totalamount += $scope.GetNumber(v.price) * $scope.GetNumber(v.qty);
                $scope.orderqty += $scope.GetNumber(v.qty);
            });
        }
        $scope.AddProducts = function () {
            $scope.totalamount = 0;
            $scope.orderqty = 0;

            if (typeof ($scope.CustomerOrder.orderitems) == 'undefined' || $scope.CustomerOrder.orderitems == null)
                $scope.CustomerOrder.orderitems = [];
            if ($scope.CustomerOrder.productid == null || $scope.CustomerOrder.productid == 0) {
                $rootScope.Toaster.Error("Please fill out Product details");
                return;
            }
            if ($scope.GetNumber($scope.CustomerOrder.qty) == 0 || $scope.GetNumber($scope.CustomerOrder.price) == 0) {
                $rootScope.Toaster.Error("Please fill out product qty and price");
                return;
            }

            var suppliername = $scope.SearchRef($scope.CustomerOrder.supplierid, $scope.Suppliers, '');
            var productname = $scope.SearchRef($scope.CustomerOrder.productid, $scope.Products, '');
            var brandname = $scope.SearchRef($scope.CustomerOrder.brandid, $scope.BrandName, '');
            var stylename = $scope.SearchRef($scope.CustomerOrder.styleid, $scope.Styles, '');
            var sizename = $scope.SearchRef($scope.CustomerOrder.sizeid, $scope.Sizes, '');
            var colorname = $scope.SearchRef($scope.CustomerOrder.colorid, $scope.Colors, '');

            var description = [];
            if (suppliername != '')
                description.push(suppliername);
            if (productname != '')
                description.push(productname);
            if (brandname != '')
                description.push(brandname);
            if (stylename != '')
                description.push(stylename);
            if (sizename != '')
                description.push(sizename);
            if (colorname != '')
                description.push(colorname);
            if ($scope.CustomerOrder.designno != '')
                description.push($scope.CustomerOrder.designno);

            $scope.CustomerOrder.orderitems.push(
                {
                    'supplierid': $scope.CustomerOrder.supplierid,
                    'productid': $scope.CustomerOrder.productid,
                    'product': $scope.SearchRef($scope.CustomerOrder.productid, $scope.Products),
                    'brandid': $scope.CustomerOrder.brandid,
                    'styleid': $scope.CustomerOrder.styleid,
                    'sizeid': $scope.CustomerOrder.sizeid,
                    'colorid': $scope.CustomerOrder.colorid,
                    'designno': $scope.CustomerOrder.designno,
                    'description': description.join('/'),
                    'qty': $scope.CustomerOrder.qty,
                    'price': $scope.CustomerOrder.price
                });

            $.each($scope.CustomerOrder.orderitems, function (k, v) {
                $scope.totalamount += $scope.GetNumber(v.price) * $scope.GetNumber(v.qty);
                $scope.orderqty += $scope.GetNumber(v.qty);
            });
        }
        $scope.ManagePayment = function (action, value) {
            $scope.paidamt = 0;
            if (action == 'delete') {
                $scope.CustomerOrder.paymentinfo.splice($scope.CustomerOrder.paymentinfo.indexOf(value), 1);
            }
            if (action == 'edit') {
                //$scope.CustomerOrder.paymentinfo.splice($scope.CustomerOrder.paymentinfo.indexOf(value), 1);
                $scope.paymentitem = value;
                if ($scope.paymentitem.ledgerid == undefined ||
                    $scope.paymentitem.ledgerid == null ||
                    $scope.paymentitem.ledgerid <= 0) {
                    if ($scope.paymentitem.bankid && $scope.paymentitem.bankid > 0 &&
                        $scope.Ledgers && $scope.Ledgers.length>0) {
                        var ledger = $scope.Ledgers.find(function (item) {
                            return item.bankid == $scope.paymentitem.bankid;
                        });
                        if (ledger && ledger != null)
                            $scope.paymentitem.ledgerid = ledger.id;
                    }
                }
            }
            $scope.paidamt = 0;
            $.each($scope.CustomerOrder.paymentinfo, function (k, v) {
                $scope.paidamt += $scope.GetNumber(v.amount);
            });

        }
        $scope.AddPayments = function () {

            if ($rootScope.LocationInfo == undefined || $rootScope.LocationInfo.location == undefined) {
                $rootScope.Toaster.Error("Please select the location");
            }

            if ($scope.CustomerOrder.paymentinfo.indexOf($scope.paymentitem) >= 0) {
                $scope.paymentitem = {};
                return;
            }

            if ($scope.CustomerOrder.locationid == undefined || $scope.CustomerOrder.locationid == 0) {
                $rootScope.Toaster.Error("Please select the location");
            }

            $scope.paidamt = 0;
            if (typeof ($scope.CustomerOrder.paymentinfo) == 'undefined' || $scope.CustomerOrder.paymentinfo == null)
                $scope.CustomerOrder.paymentinfo = [];

            $scope.CustomerOrder.paymentinfo.push(
                {
                    'actionon': $scope.DateToday,
                    'amount': $scope.paymentitem.amount,
                    'notes': $scope.paymentitem.notes,
                    'mode': $scope.paymentitem.mode,
                    'receivedby': $scope.paymentitem.receivedby,
                    'ledgerid': $scope.paymentitem.ledgerid,
                    'bankid': $scope.paymentitem.bankid,
                    'chequedate': $scope.paymentitem.chequedate,
                    'referencedetail': $scope.paymentitem.referencedetail,
                    'remarks': $scope.paymentitem.remarks,
                    'counterid': $scope.paymentitem.counterid,
                    'locationid': $scope.CustomerOrder.locationid
                });
            $scope.paymentitem = {};
            $.each($scope.CustomerOrder.paymentinfo, function (k, v) {
                $scope.paidamt += $scope.GetNumber(v.amount);
            });
        }

        $scope.exporttoExcel = function (infygrid, title) {
            //var title = $rootScope.UserInfo.CompanyName;
            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, '');
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = filename + ".xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }
        $scope.AddCommunication = function () {
            if (typeof ($scope.CustomerOrder.communication) == 'undefined' || $scope.CustomerOrder.communication == null)
                $scope.CustomerOrder.communication = [];
            $scope.CustomerOrder.communication.push(
                {
                    'spokenwith': $scope.CustomerOrder.spokenwith,
                    'notes': $scope.CustomerOrder.notes,
                    'iscustomer': $scope.CustomerOrder.iscustomer,
                    'actionon': $scope.CustomerOrder.communicatedon

                });
        }


        $scope.PrintOrder = function (id) {
            $rootScope.Download('CustomerorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.PurchaseReturn.id : id), 'customerlabel.pdf', 'application/pdf');
            return;

            $timeout(function () {
                var link = document.createElement('a');
                link.download = "order.pdf";
                link.href = 'CustomerorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.PurchaseReturn.id : id);
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }
        $scope.PrintItem = function (orderid, itemid) {
            $rootScope.Download('CustomerorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (orderid) == 'undefined' ? $scope.PurchaseReturn.id : orderid) + '/' + itemid, 'customerlabel.pdf', 'application/pdf');
            return;
        }

        $scope.Print = function (id) {
            request = {
                "Item": { 'id': id },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);


            $http({
                method: 'POST',
                url: "CustomerorderService/Print",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0 && result.printdocument != null) {
                    $scope.Sent2Printer(result.printdocument);
                }


            }).error(function (result) {
                $scope.ErrorMessage = result;
            });
        }

        $scope.Sent2Printer = function (printdocument, printdata) {
            if (printdocument == null)
                return;

            var printerData = typeof (printdata) == 'undefined' ? localStorageService.get('PrinterData') : printdata;
            if (printerData == null || typeof (printerData) == 'undefined') {
                $rootScope.Toaster.Error("Please select the printer");
                return;
            }

            $rootScope.Sent2Printer(printdocument, printerData);

            //var request = {
            //    'id': printerData.printerid,
            //    'content': printdocument,
            //    'encoded': 'BASE64'
            //};
            //var srequest = JSON.stringify(request);
            //$http({
            //    method: 'POST',
            //    url: printerData.service,
            //    data: srequest
            //}).success(function (result) {

            //});
        }

        $scope.SearchCustomer = function () {
            //if ($scope.CustomerOrder.contactno == null || $scope.CustomerOrder.contactno == '') {
            //    $scope['setFocusfirst']();
            //    $rootScope.Toaster.Error("Please Enter Customer Mobile Number...!");
            //    return;
            //}
            //request = {
            //    "item": { mobileno: $scope.CustomerOrder.contactno },
            //    "securitytoken": "String content"
            //}

            //var srequest = JSON.stringify(request);

            //$http({
            //    method: 'POST',
            //    url: "CustomerService/Search",
            //    data: srequest
            //}).success(function (result) {
            //    $scope.CustomerOrder.area = result[0].area;
            //    $scope.CustomerOrder.address = result[0].address;
            //    $scope.CustomerOrder.name = result[0].name;
            //    $scope.CustomerOrder.customerid = result[0].id;

            //}).error(function (result) {
            //});

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
                        $scope.setFocuscustomer_name();
                    }, 10);

                    $scope.contactno = $scope.ngDialogData[0].CustomerOrder.contactno;
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
                        //$scope.SearchPaymentCondition.customerid = data.value.id;
                        //$scope.SearchPaymentCondition.customername = data.value.name;
                        $scope.CustomerOrder.cityid = data.value.cityid;
                        $scope.CustomerOrder.address = data.value.address;
                        $scope.CustomerOrder.name = data.value.name;
                        $scope.CustomerOrder.customerid = data.value.id;
                        $scope.CustomerOrder.contactno = data.value.mobileno;
                    }
                }
            });
        }

        $scope.Completed = function (status) {
            $scope.CustomerOrder.status = status;

            var request = {
                "Item": $scope.CustomerOrder,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerorderService/UpdateStatus",
                data: request
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Status Updated Successfully.");
                    $scope.CustomerOrder = result;
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });

            if (status == '3')
                $scope.New();
        }
        $scope.SaveOrder = function () {

            if ($scope.CustomerOrder.contactno == null || $scope.CustomerOrder.contactno == "" ||
                $scope.CustomerOrder.name == null || $scope.CustomerOrder.name == "") {
                $rootScope.Toaster.Error("Please fill customer  details");
                return;
            }
            if ($scope.CustomerOrder.orderitems == null || $scope.CustomerOrder.orderitems.length == 0) {
                $rootScope.Toaster.Error("Please fill customer order details");
                return;
            }

            if ($scope.CustomerOrder.paymentinfo == null || $scope.CustomerOrder.paymentinfo.length == 0) {
                $rootScope.Toaster.Error("Please fill customer payment details");
                return;
            }

            /*$scope.CustomerOrder.paymentinfo = [];
            $scope.CustomerOrder.paymentinfo.push(
                {
                    'actionon': $scope.CustomerOrder.actionon,
                    'amount': $scope.CustomerOrder.amount,
                    'notes': $scope.CustomerOrder.notes,
                    'mode': $scope.CustomerOrder.paymentmode,
                    'bankid': $scope.CustomerOrder.bankid,
                    'chequedate': $scope.CustomerOrder.chequedate,
                    'referencedetail': $scope.CustomerOrder.referencedetail,
                    'remarks': $scope.CustomerOrder.remarks,
                    'counterid': $rootScope.LocationInfo.counter.id
                });*/

            var request = {
                "Item": $scope.CustomerOrder,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerorderService/Save",
                data: request
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Customer Saved Successfully !! With The Customer Order Number = " + result.customerorderno);
                    $scope.CustomerOrder = result;
                    //if ($scope.CustomerOrder.paymentinfo != null && $scope.CustomerOrder.paymentinfo.length > 0) {
                    //    $scope.CustomerOrder.actionon = $scope.CustomerOrder.paymentinfo[0].actionon;
                    //    $scope.CustomerOrder.paymentmode = $scope.CustomerOrder.paymentinfo[0].mode;
                    //    $scope.CustomerOrder.bankid = $scope.CustomerOrder.paymentinfo[0].bankid;
                    //    $scope.CustomerOrder.chequedate = $scope.CustomerOrder.paymentinfo[0].chequedate;
                    //    $scope.CustomerOrder.referencedetail = $scope.CustomerOrder.paymentinfo[0].referencedetail;
                    //    $scope.CustomerOrder.amount = $scope.CustomerOrder.paymentinfo[0].amount;
                    //    $scope.CustomerOrder.remarks = $scope.CustomerOrder.paymentinfo[0].remarks;
                    //}
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });

        }

        $scope.SearchOrders = function () {
            $scope.totalamount = 0;
            $scope.paidamt = 0;
            $scope.orderqty = 0;
            request = {
                "Item": $scope.SearchRequest,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerorderService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.OrderConfig();
            }).error(function (result) {
            });

        }
        $scope.SearchOrders();

        $scope.ManageOrderList = function (item) {
            $scope.ShowSearch = true;
            $scope.CustomerOrder = item;
            $scope.FilterCounter(item.locationid);
            $scope.paidamt = 0;
            $scope.totalamount = 0;
            $scope.orderqty = 0;
            $.each($scope.CustomerOrder.paymentinfo, function (k, v) {
                $scope.paidamt += $scope.GetNumber(v.amount);
            });
            $.each($scope.CustomerOrder.orderitems, function (k, v) {
                $scope.totalamount += $scope.GetNumber(v.price) * $scope.GetNumber(v.qty);
            });
            $.each($scope.CustomerOrder.orderitems, function (k, v) {
                $scope.orderqty += $scope.GetNumber(v.qty);
            });

            $.each($scope.CustomerOrder.orderitems, function (k, v) {
                var suppliername = $scope.SearchRef(v.supplierid, $scope.Suppliers, '');
                var productname = $scope.SearchRef(v.productid, $scope.Products, '');
                var brandname = $scope.SearchRef(v.brandid, $scope.BrandName, '');
                var stylename = $scope.SearchRef(v.styleid, $scope.Styles, '');
                var sizename = $scope.SearchRef(v.sizeid, $scope.Sizes, '');
                var colorname = $scope.SearchRef(v.colorid, $scope.Colors, '');

                var description = [];
                if (suppliername != '')
                    description.push(suppliername);
                if (productname != '')
                    description.push(productname);
                if (brandname != '')
                    description.push(brandname);
                if (stylename != '')
                    description.push(stylename);
                if (sizename != '')
                    description.push(sizename);
                if (colorname != '')
                    description.push(colorname);
                if (v.designno != '')
                    description.push(v.designno);
                v.description = description.join('/')
            });

            //if ($scope.CustomerOrder.paymentinfo != null && $scope.CustomerOrder.paymentinfo.length > 0)
            //{
            //    $scope.CustomerOrder.actionon = $scope.CustomerOrder.paymentinfo[0].actionon;
            //    $scope.CustomerOrder.paymentmode = $scope.CustomerOrder.paymentinfo[0].mode;
            //    $scope.CustomerOrder.bankid = $scope.CustomerOrder.paymentinfo[0].bankid;
            //    $scope.CustomerOrder.chequedate = $scope.CustomerOrder.paymentinfo[0].chequedate;
            //    $scope.CustomerOrder.referencedetail = $scope.CustomerOrder.paymentinfo[0].referencedetail;
            //    $scope.CustomerOrder.amount = $scope.CustomerOrder.paymentinfo[0].amount;
            //    $scope.CustomerOrder.remarks = $scope.CustomerOrder.paymentinfo[0].remarks;
            //}
        };


        $scope.OrderConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 40);

            igrid.AddSimpleColumn('customerorderno', 'ORDER NO', 80);
            igrid.AddFormatedColumn('orderdate', 'ORDER DATE', 120, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('deliverydate', 'DELIVERY DATE', 140, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('name', 'CUSTOMER NAME', 150);
            igrid.AddSimpleColumn('contactno', 'CONTACT NO', 110, 'N2', 2);
            igrid.AddReferenceColumn('cityid', 'CITY', 150, $scope.Cities);
            igrid.AddReferenceColumn('categoryid', 'CATEGORY', 120, $scope.CustomerCategory);
            igrid.AddSimpleColumn('ordertakenby', 'ORDER RECD. BY', 120);
            igrid.AddFormatedColumn('totalqty', 'TOTAL QTY', 100, 'N2', 2);
            igrid.AddFormatedColumn('orderamount', 'TOTAL AMOUNT', 120, 'N2', 2);
            igrid.AddFormatedColumn('paidamount', 'PAID AMOUNT', 120, 'N2', 2);
            igrid.AddReferenceColumn('status', 'STATUS', 100, $scope.StatusList);
            igrid.AddButtonColumn('action', 'Action', 100, 'Manage', function (e, args) {
                $scope.$apply(function () {
                    $scope.ManageOrderList(args.item);
                });
            });
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);


    }]);

