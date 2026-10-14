var app = angular.module('InfinityLab');

app.controller('SalesDashboardController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', '$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, $window,$stateParams) {

        $rootScope.AccordionName = $location.path();


    }]);