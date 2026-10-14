
var app = angular.module('InfinityLab');

app.controller('DefaultReferencesController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;

        $scope.OrganizationSelected = false;
        $scope.SearchOrganization = function () {
            $scope.OrganizationSelected = false;
            $scope.OrganizationName = "";
            restService.Get('OrganisationService/Search/' + $scope.organizationcode, true).then(function (results) {
                $scope.CurrentOrganization = results;
                if ($scope.CurrentOrganization != null) {
                    $scope.OrganizationSelected = true;
                    $scope.OrganizationName = $scope.CurrentOrganization.name;
                }
            });
        }

        $scope.Save = function () {

            if ($scope.Brand.code == null || $scope.Brand.code == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please select Code");
                return;
            }

            if ($scope.Brand.name == null || $scope.Brand.name == 0) {
                $scope['setFocusname']();
                $rootScope.Toaster.Error("Please select Name");
                return;
            }

            if ($scope.Brand.printingname == null || $scope.Brand.printingname == 0) {
                $scope['setFocusprintingname']();
                $rootScope.Toaster.Error("Please select Printing Name");
                return;
            }

            //$scope.Brand.patterns = { items: [] };
            //$.each($scope.patterns, function (k, v) {
            //    if (v.selected)
            //        $scope.Brand.patterns.items.push({ 'name': v.name, 'id': v.id, 'available': true, 'ro':v.ro });
            //});
            //$scope.Brand.sizes = { items: [] };
            //$.each($scope.sizes, function (k, v) {
            //    if (v.selected)
            //        $scope.Brand.sizes.items.push({ 'name': v.name, 'id': v.id, 'available': true, 'ro':v.ro });
            //});
            //$scope.Brand.styles = { items: [] };
            //$.each($scope.styles, function (k, v) {
            //    if (v.selected)
            //        $scope.Brand.styles.items.push({ 'name': v.name, 'id': v.id, 'available': true, 'ro':v.ro });
            //});
            //$scope.Brand.materials = { items: [] };
            //$.each($scope.materials, function (k, v) {
            //    if (v.selected)
            //        $scope.Brand.materials.items.push({ 'name': v.name, 'id': v.id, 'available': true, 'ro':v.ro });
            //});

            //$scope.Brand.colours = { items: [] };
            //$.each($scope.ColorList, function (k, v) {
            //    if (v.selected)
            //        $scope.Brand.colours.items.push({ 'name': v.name, 'id': v.id, 'available': true, 'ro':v.ro });
            //});
            request = {
                "Item": $scope.Brand,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BrandService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {

                    $rootScope.Toaster.Info("Brand Saved Successfully.!");
                    $scope.Brand = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

                }

            }).error(function (result) {
            });

        }

    }]);



