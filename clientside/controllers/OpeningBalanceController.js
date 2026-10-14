
var app = angular.module('InfinityLab');

app.controller('OpningBalanceController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter', 'ngDialog', 'localStorageService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter, ngDialog, localStorageService) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.Navigation.SetMode('SEARCH');
        $scope.LedgerTypeList = [{ 'id': 0, 'name': 'Suppliers' }, { 'id': 1, 'name': 'Transports' },
        { 'id': 2, 'name': 'Customers' }];

        $scope.PaymentModes = [{ 'id': 1, 'name': 'Inter Bank Transfer' },
        { 'id': 2, 'name': 'Cheque/DD' },
        { 'id': 3, 'name': 'Cash' },
        { 'id': 4, 'name': 'Card' }];

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
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
            OpeningBalanceno: "",
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
            area: 0,
            status: 0,
            totalqty: ""

        }
        $scope.ShowSearch = false;

        $scope.New = function () {
            $scope.OpeningBalance = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.OpeningBalance.orderitems = [];
            $scope.OpeningBalance.paymentinfo = [];
            $scope.OpeningBalance.communication = [];

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
                $scope.OpeningBalance.orderitems.splice($scope.OpeningBalance.orderitems.indexOf(value), 1);
            }
            $scope.totalamount = 0;
            $scope.orderqty = 0;
            $.each($scope.OpeningBalance.orderitems, function (k, v) {
                $scope.totalamount += $scope.GetNumber(v.price) * $scope.GetNumber(v.qty);
                $scope.orderqty += $scope.GetNumber(v.qty);
            });
        }
        $scope.AddProducts = function () {
            $scope.totalamount = 0;
            $scope.orderqty = 0;

            if (typeof ($scope.OpeningBalance.orderitems) == 'undefined' || $scope.OpeningBalance.orderitems == null)
                $scope.OpeningBalance.orderitems = [];
            if ($scope.OpeningBalance.productid == null || $scope.OpeningBalance.productid == 0) {
                $rootScope.Toaster.Error("Please fill out Product details");
                return;
            }
            if ($scope.GetNumber($scope.OpeningBalance.qty) == 0 || $scope.GetNumber($scope.OpeningBalance.price) == 0) {
                $rootScope.Toaster.Error("Please fill out product qty and price");
                return;
            }

            $scope.OpeningBalance.orderitems.push(
                {
                    'supplierid': $scope.OpeningBalance.supplierid,
                    'productid': $scope.OpeningBalance.productid,
                    'product': $scope.SearchRef($scope.OpeningBalance.productid, $scope.Products),
                    'brandid': $scope.OpeningBalance.brandid,
                    'styleid': $scope.OpeningBalance.styleid,
                    'sizeid': $scope.OpeningBalance.sizeid,
                    'colorid': $scope.OpeningBalance.colorid,
                    'designno': $scope.OpeningBalance.designno,
                    'qty': $scope.OpeningBalance.qty,
                    'price': $scope.OpeningBalance.price
                });

            $.each($scope.OpeningBalance.orderitems, function (k, v) {
                $scope.totalamount += $scope.GetNumber(v.price) * $scope.GetNumber(v.qty);
                $scope.orderqty += $scope.GetNumber(v.qty);
            });
        }
      


       
        $scope.Completed = function (status) {
            $scope.OpeningBalance.status = status;
            $scope.SaveOrder();
        }
        $scope.SaveOpeningBalance = function () {

            var request = {
                "Item": $scope.OpeningBalance,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "OpeningbalancesService/Save",
                data: request
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Customer Saved Successfully !! With The Customer Order Number = " + result.OpeningBalanceno);
                    $scope.OpeningBalance = result;
                    //if ($scope.OpeningBalance.paymentinfo != null && $scope.OpeningBalance.paymentinfo.length > 0) {
                    //    $scope.OpeningBalance.actionon = $scope.OpeningBalance.paymentinfo[0].actionon;
                    //    $scope.OpeningBalance.paymentmode = $scope.OpeningBalance.paymentinfo[0].mode;
                    //    $scope.OpeningBalance.bankid = $scope.OpeningBalance.paymentinfo[0].bankid;
                    //    $scope.OpeningBalance.chequedate = $scope.OpeningBalance.paymentinfo[0].chequedate;
                    //    $scope.OpeningBalance.referencedetail = $scope.OpeningBalance.paymentinfo[0].referencedetail;
                    //    $scope.OpeningBalance.amount = $scope.OpeningBalance.paymentinfo[0].amount;
                    //    $scope.OpeningBalance.remarks = $scope.OpeningBalance.paymentinfo[0].remarks;
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
                url: "OpeningbalancesService/Search",
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
            $scope.OpeningBalance = item;
            $scope.paidamt = 0;
            $scope.totalamount = 0;
            $scope.orderqty = 0;
            $.each($scope.OpeningBalance.paymentinfo, function (k, v) {
                $scope.paidamt += v.amount;
            });
            $.each($scope.OpeningBalance.orderitems, function (k, v) {
                $scope.totalamount += v.price * v.qty;
            });
            $.each($scope.OpeningBalance.orderitems, function (k, v) {
                $scope.orderqty += v.qty;
            });

            //if ($scope.OpeningBalance.paymentinfo != null && $scope.OpeningBalance.paymentinfo.length > 0)
            //{
            //    $scope.OpeningBalance.actionon = $scope.OpeningBalance.paymentinfo[0].actionon;
            //    $scope.OpeningBalance.paymentmode = $scope.OpeningBalance.paymentinfo[0].mode;
            //    $scope.OpeningBalance.bankid = $scope.OpeningBalance.paymentinfo[0].bankid;
            //    $scope.OpeningBalance.chequedate = $scope.OpeningBalance.paymentinfo[0].chequedate;
            //    $scope.OpeningBalance.referencedetail = $scope.OpeningBalance.paymentinfo[0].referencedetail;
            //    $scope.OpeningBalance.amount = $scope.OpeningBalance.paymentinfo[0].amount;
            //    $scope.OpeningBalance.remarks = $scope.OpeningBalance.paymentinfo[0].remarks;
            //}
        };


        $scope.OrderConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 40);
            igrid.AddReferenceColumn('ledgertype', 'LEDGER TYPE', 150, $scope.LedgerTypeList);

            igrid.AddSimpleColumn('opening', 'OPENING BALANCE', 150);

            igrid.AddSimpleColumn('financeyear', 'YEAR', 150);
            igrid.AddSimpleColumn('closing', 'OPENING BALANCE', 150);

            igrid.AddReferenceColumn('ledgerid', 'SUPPLIERS', 100, $scope.Suppliers);
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

