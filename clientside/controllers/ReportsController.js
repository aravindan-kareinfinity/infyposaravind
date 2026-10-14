
var app = angular.module('InfinityLab');

app.controller('ReportsController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService',
    function(UserDetails, $scope, $http, $location, $rootScope, $window, restService) {
        $rootScope.AccordionName = $location.path();
        $scope.Permission = UserDetails.Info;
        $scope.heading = "Purchase";
        $scope.currentDate = new Date();
        $scope.SearchRef = restService.SearchReference;

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function(results) {
            $scope.Suppliers = results;
        });

        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function(results) {
            $scope.BuyingCompany = results;
        });
        $scope.customer = false;
        $scope.supplier = true;

        $scope.Purchase = true;
        $scope.PurchaseReturn = false;
        $scope.Sales = false;
        $scope.SalesReturn = false;
        $scope.Tax = false;




        $scope.printDiv = function(printSectionId) {
            var printContents = document.getElementById(printSectionId).innerHTML;
            var originalContents = document.body.innerHTML;
            document.body.innerHTML = printContents;
            window.print();
            document.body.innerHTML = originalContents;

        };

    }]);
app.controller('Reports_PurchaseReturnController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService',
    function(UserDetails, $scope, $http, $location, $rootScope, $window, restService) {
        $rootScope.AccordionName = $location.path();
        $scope.heading = "PurchaseReturn";


        $scope.SearchRef = restService.SearchReference;

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function(results) {
            $scope.Suppliers = results;
        });

        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function(results) {
            $scope.BuyingCompany = results;
        });

        $scope.customer = false;
        $scope.supplier = true;

        $scope.Purchase = false;
        $scope.PurchaseReturn = true;
        $scope.Sales = false;
        $scope.SalesReturn = false;
        $scope.Tax = false;






    }]);

app.controller('Reports_SalesController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService',
    function(UserDetails, $scope, $http, $location, $rootScope, $window, restService) {
        $rootScope.AccordionName = $location.path();

        $scope.heading = "Sales";

        $scope.SearchRef = restService.SearchReference;

        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function(results) {
            $scope.BuyingCompany = results;
        });

        $scope.customer = true;
        $scope.supplier = false;

        $scope.Purchase = false;
        $scope.PurchaseReturn = false;
        $scope.Sales = true;
        $scope.SalesReturn = false;
        $scope.Tax = false;




    }]);



app.controller('Reports_SalesReturnController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService',
    function(UserDetails, $scope, $http, $location, $rootScope, $window, restService) {
        $rootScope.AccordionName = $location.path();

        $scope.heading = "SalesReturn";

        $scope.SearchRef = restService.SearchReference;

        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function(results) {
            $scope.BuyingCompany = results;
        });
        $scope.customer = true;
        $scope.supplier = false;

        $scope.Purchase = false;
        $scope.PurchaseReturn = false;
        $scope.Sales = false;
        $scope.SalesReturn = true;
        $scope.Tax = false;





    }]);

app.controller('Reports_TaxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService',
    function(UserDetails, $scope, $http, $location, $rootScope, $window, restService) {
        $rootScope.AccordionName = $location.path();

        $scope.heading = "Tax";


        $scope.SearchRef = restService.SearchReference;

        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function(results) {
            $scope.BuyingCompany = results;
        });

        $scope.customer = true;
        $scope.supplier = false;

        $scope.Purchase = false;
        $scope.PurchaseReturn = false;
        $scope.Sales = false;
        $scope.SalesReturn = false;
        $scope.Tax = true;



    }]);