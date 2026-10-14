
var app = angular.module('InfinityLab');

app.controller('SettlementDashboardController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window',
    function (UserDetails, $scope, $http, $location, $rootScope, $window) {

        $rootScope.AccordionName = $location.path();


    }]);