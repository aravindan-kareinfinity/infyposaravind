
var app = angular.module('InfinityLab');

app.controller('SalesreturnController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService','localStorageService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService,localStorageService) {
        $rootScope.AccordionName = $location.path();

        $scope.locationid = null;

        // $scope.Cites = [{ 'ID': 1, 'Name': 'Chennai' }, { 'ID': 2, 'Name': 'Cuddalore' }, { 'ID': 3, 'Name': 'Panruti' }, { 'ID': 4, 'Name': 't.nager' }];
        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });
        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.SearchRef = restService.SearchReference;
        $scope.Employee = [{ 'id': 1, 'name': 'Professional Courier' }, { 'id': 2, 'name': 'BlueDart Courier' }, { 'id': 3, 'name': 'KPN Parcel Service' }];
        $scope.Counters = [{ 'id': 1, 'name': 'counter1' }, { 'id': 2, 'name': 'counter1' }, { 'id': 3, 'name': 'counter1' }];
        $scope.Entity = {
            "id": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            "billid": 0,
            "billno": null,
            "billdate": "",
            "barcode":0,
            "returndate": "",
            "companyid": 0,
            "billamount": 0.0,
            "tax": 0.0,
            "taxpercentage": 0.0,
            "discount": 0.0,
            "payable": 0.0,
            "notes": null,
            "approvedby": 0,
            "customerid": 0,
            "vouchercode": null,
            "customername": null,
            "returnqty": [{ rqty: "" }],
            "Billreturnitems": [],
            "currentbilltax":[],
            "returnitems": [{ amount: "" }]
        };

        $scope.SalesReturn = (JSON.parse(JSON.stringify($scope.Entity)));

        //$scope.BillItem = [{ barcode: "ABCD", itemname: "Shirt", stockid: 0, itemid: 0 }, { barcode: "XYZ", itemname: "Phant", stockid: 0, itemid: 0 }];

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.SalesReturn = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.Print = function (item) {
            if (typeof (item.printdocument) != 'undefined' &&
                item.printdocument != null && item.printdocument.length > 0) {
                $scope.Sent2Printer(item.printdocument);
                return;
            }

            var request = {
                "Item": { 'id': item.id, 'enableprintdocument': true },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillreturnService/Search",
                data: srequest
            }).success(function (result) {

                if (result != null && result.length == 1) {
                    $scope.Sent2Printer(result[0].printdocument);
                }
            });
        };

        $scope.Sent2Printer = function (printdocument) {

            $rootScope.Sent2Printer(printdocument);
            //var printerData = localStorageService.get('PrinterData');
            //if (printerData == null) {
            //    return;
            //}
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

        $scope.ReturnAmountChanged = function (item) {
            if (typeof (item.amount) == 'undefined')
                item.amount = 0;

            var rqty = 0;
            if (typeof (item.rqty) == 'undefined')
                rqty = 0;
            else
                rqty = parseFloat(item.rqty);

            if (item.rqty < 0)
            {
                rqty = 0;
            } else if (item.rqty > item.qty)
            {
                rqty = 0;
            }
            $scope.SalesReturn.receivable = 0;
            $scope.SalesReturn.discount = 0;
            $scope.SalesReturn.qty = 0;
            $scope.SalesReturn.tax = 0;

            item.receivable = (item.price * rqty).round(2);
            item.tax = (item.otax / item.qty * rqty ).round(2);
            item.saleamount = (item.osaleamount / item.qty * rqty).round(2);

            $.each($scope.SalesReturn.Billreturnitems, function (k, v) {               
                $scope.SalesReturn.receivable += v.receivable;
                if (v.discount>0 && rqty>0)
                    $scope.SalesReturn.discount += (v.discount / v.qty) * rqty;
                $scope.SalesReturn.qty += rqty;
                $scope.SalesReturn.tax += v.tax;
            });
            $scope.SalesReturn.receivable = Math.round($scope.SalesReturn.receivable, 0);
            $scope.SalesReturn.payable = Math.round($scope.SalesReturn.receivable, 0);
            return true;
        };

        $scope.ErrorMessage = "";



        $scope.SearchCondition = {
            billno: "",
            barcode: 0
        }

        $scope.Save = function () {
           
            $scope.SalesReturn.locationid = $scope.locationid;
            request = {
                "Item": $scope.SalesReturn,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillreturnService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $scope.ErrorMessage = "Successfully saved";
                    $scope.ErrorStyle = "success";
                    $scope.SalesReturn.vouchercode = result.vouchercode;
                    $scope.SalesReturn.srno = result.srno;
                    $scope.SalesReturn.id = result.id;
                    $scope.SalesReturn.version = result.version;
                    if (typeof (result.printdocument) != 'undefined' || result.printdocument != null)
                        $scope.Print(result.printdocument);
                    $scope.CreateNew();
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });

        }


        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {


                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.name + "?",
                    confirmButton: 'Yes',
                    confirmButtonClass: 'btn-info',
                    icon: 'fa fa-question-circle',
                    animation: 'scale',
                    animationClose: 'top',
                    opacity: 0.5,
                    confirm: function () {

                        request = {
                            "Item": item,
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BillreturnService/Delete",
                            data: srequest
                        }).success(function (result) {

                            if (result) {
                                $scope.ErrorMessage = "Successfully deleted";
                                $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                            }

                        }).error(function (result) {

                        });
                    }
                });


            }
            else if (action == "Edit") {
                $scope.ShowSearch = false;
                $scope.denomination = item;
            }
        }

        $scope.SearchSR = function () {
            var request = {
                "Item": { 'srno': $scope.searchsrno },
                "securitytoken": "String content"
            };

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillreturnService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SalesReturn = result[0];
                $.each($scope.SalesReturn.Billreturnitems, function (k, v) {
                    v.rqty = v.qty;
                    v.price = v.rate + v.tax;
                    v.amount = v.qty * v.price;
                });
            }).error(function (result) {
            });
        }

        $scope.CheckLocation = function () {

            var printerData = localStorageService.get('LocationData');
            if (printerData != null) {
                $scope.locationid = printerData.location.id;
            }
            if ($scope.locationid == null) {
                $rootScope.ShowAlert('Please select the location', 'No-Location');
                return false;
            }
            return true;
        }

        $scope.CreateNew = function()
        {
            $scope.SalesReturn = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.setFocussearchbillno();
        }

        $scope.Search = function (mode) {

            if (!$scope.CheckLocation())
                return;

            var request = {
                "Item": {'forsalesreturn':true,locationid:$scope.locationid},
                "securitytoken": "String content"
            };

            if (mode == 'BARCODE')
                request.item.barcode = $scope.SalesReturn.barcode;
            else
                request.item.billno = $scope.SalesReturn.billno;

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillService/Search",
                data: srequest
            }).success(function (result) {  
                $scope.SalesReturn.id = 0;
                //$scope.SalesReturn = result[0];
                $scope.SalesReturn.billid = result[0].id;
                $scope.SalesReturn.billno = result[0].billno;
                $scope.SalesReturn.billdate = result[0].billdate;
                $scope.SalesReturn.customername = result[0].customername;
                //$scope.SalesReturn.companyid = result[0].companyid;
                $scope.SalesReturn.customername = result[0].customername;
                $scope.SalesReturn.currentbilltax = result[0].BillTax;
                $scope.SalesReturn.Billreturnitems = result[0].Billitems;
                $scope.SalesReturn.receivable = 0;
                $.each($scope.SalesReturn.Billreturnitems, function (k, v) {
                        v.rqty = 0;
                        v.receivable = 0;
                        v.billitemsid = v.id;
                        v.osaleamount = v.saleamount;
                        v.otax = v.tax;
                });
            }).error(function (result) {
            });

        }
    }]);