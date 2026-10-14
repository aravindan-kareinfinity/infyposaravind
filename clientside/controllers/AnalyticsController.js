
var app = angular.module('InfinityLab');

app.controller('Analytics_MovementController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService) {
        $rootScope.AccordionName = $location.path();

        $scope.heading = "Purchase";

        $scope.SearchRef = restService.SearchReference;

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });
        
        $scope.printDiv = function (printSectionId) {
            var printContents = document.getElementById(printSectionId).innerHTML;
            var originalContents = document.body.innerHTML;
            document.body.innerHTML = printContents;
            window.print();
            document.body.innerHTML = originalContents;
          
        };
     
    }]);
