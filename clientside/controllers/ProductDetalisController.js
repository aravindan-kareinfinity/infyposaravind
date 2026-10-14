
var app = angular.module('InfinityLab');

app.controller('ProductDetalisController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window','restService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService) {

        $scope.ProductList= [{
            LRid: 0,
            Product: "Shirt",
            Qty: "2",
            BuyingPrice	: "1000",
            MRP: "950",
            SellingPrice:"1250"	
        }];

        //$scope.Items = [{ 'ID': 1, 'Name': 'item1' }, { 'ID': 2, 'Name': 'item2' }, { 'ID': 3, 'Name': 'item3' }, { 'ID': 4, 'Name': 'item4' }];


        $scope.ItemName = restService.Get('api/ReferencelistService/ItemName', true).then(function (results) {
            $scope.ItemName = results;
        });


        $scope.Statuses = restService.Get('api/ReferencelistService/Statuses', true).then(function (results) {
            $scope.Statuses = results;
        });


        // model to be used to Productdetails
        $scope.Productdetails = {
            SupplierCompany: "",
            BuyingCompany: "",
            InvoiceNoDate: "",
            PurchaseOrderNo: "",
            NoofPieces: "",
            NoofBundles: "",
            BaseAmount: "",
            GrossAmount: "",
            Status: "",
            version: "1",
            Products: [{ 'Product': "", 'Qty': "", 'Buying Price': "", 'MRP': "", 'Selling Price': "", 'Action': "" }]
        };


    }]);
        