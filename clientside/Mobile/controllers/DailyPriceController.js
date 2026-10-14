
var app = angular.module('InfinityLab');

app.controller('MobileDailyPriceController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Navigation.SetMode('SEARCH');

        $scope.StatusList = [{ 'id': 0, 'name': 'Ordered' }, { 'id': 1, 'name': 'Progress' },
        { 'id': 2, 'name': 'Completed' }, { 'id': 3, 'name': 'Cancelled' }, { 'id': 4, 'name': 'Rejected' }];
        $scope.UnitsList = [{ 'id': 0, 'name': '1' }, { 'id': 1, 'name': '2' },
        { 'id': 2, 'name': '3' }, { 'id': 3, 'name': '4' }];
        $scope.Reasons = [{ 'id': 0, 'name': 'Damaged' }, { 'id': 1, 'name': 'ScreenProblem' },
        { 'id': 2, 'name': 'Mic Problem' }, { 'id': 3, 'name': 'Others' }];
        $scope.WarrentyList = [{ 'id': 0, 'name': '6 Months' }, { 'id': 1, 'name': '1 Year' },
        { 'id': 2, 'name': '2 Years' }, { 'id': 3, 'name': '3 Years' }];

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.BrandName = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandName = results;
        });

        $scope.Colors = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Colors = results;
        });

        $scope.Cities = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.Cities = results;
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
        $scope.Entity =
            {
                id: "",
                invoiceno: "",
                customerid: "",
                imeino: "",
                modelno: "",
                productid: "",
                brandid: "",
                invoiceitemid: "",
                status: "",
                approvedby: "",
                approveddate: "",
                reason: "",
                units: "",
                warrenty: "",
                colourid: "",
                amount: "",
                isactive: true,
                createdby: "",
                modifiedby: "",
                version: "",
                createdon: new Date(),
                modifiedon: new Date,
                referenceno: "",
                locationid: "",
                remarks: ""

            }
        $scope.DOA = (JSON.parse(JSON.stringify($scope.Entity)));

        
        $scope.SearchOrders = function () {
            $scope.totalamount = 0;
            $scope.paidamt = 0;
            $scope.orderqty = 0;
            request = {
                "Item": $scope.DOA,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DOAService/Search",
                data: srequest
            }).success(function (result) {
                $scope.DOA = result[0];
                $.each(result[0].paymentinfo, function (k, v) {
                    $scope.paidamt += v.amount;
                });
                $.each(result[0].orderitems, function (k, v) {
                    $scope.totalamount += v.price * v.qty;
                });
                $.each(result[0].orderitems, function (k, v) {
                    $scope.orderqty += v.qty;
                });
                $scope.OrderConfig();
            }).error(function (result) {
            });

        }
        

        $scope.OrderConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true);

            igrid.AddSimpleColumn('name', 'NAME', 100);
            igrid.AddSimpleColumn('barcode', 'BARCODE', 100);
            igrid.AddSimpleColumn('stock', 'STOCK', 100);
            igrid.AddSimpleColumn('current', 'CURRENT', 100);
            igrid.AddSimpleColumn('war', 'WAR', 100);
            var pricecolumn = igrid.AddSimpleColumn('newprice', 'NEW PRICE', 100);
            pricecolumn.editor = Slick.Editors.NumberEditor;

           
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);


    }]);

