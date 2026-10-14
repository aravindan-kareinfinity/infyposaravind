
var app = angular.module('InfinityLab');

app.controller('StockReportController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService','$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

   
        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });
        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.ColorList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });
        $scope.BrandName = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandName = results;
        });

        $scope.patterns = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.patterns = results;
        });

        $scope.sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.sizes = results;
        });

        $scope.materials = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.materials = results;
        });

        $scope.styles = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.styles = results;
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });

        $scope.ColorList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });
        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });
        $scope.TypeList = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.TypeList = results;
        });

        $scope.SearchResult = [];
        $scope.SearchRetailResult = [];


     
        $scope.SearchCondition = {
            companyid: "",
            productid: "",
            supplierid: "",
            qty:""
        }

        $scope.Search = function () {

            request = {
                "Item": {
                    companyid: $scope.companyid,
                    productid: $scope.productid,
                    supplierid: $scope.supplierid,
                    qty: $scope.qty
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "StockService/SearchStock",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
            }).error(function (result) {
            });

        }

        $scope.RetailStockSearch = function () {

            request = {
                "Item": {
                    companyid: $scope.companyid,
                    productid: $scope.productid,
                    supplierid: $scope.supplierid,
                    locationid:$scope.locationid,
                    qty: $scope.qty
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "StockService/SearchRetailStock",
                data: srequest
            }).success(function (result) {

                $scope.SearchRetailResult = result;
            }).error(function (result) {
            });

        }

        $scope.printDiv = function (div, title) {

            var otitle = angular.element('#apptitle').context.title;
            var titlele = angular.element('#apptitle').context.title = " Stock Report for ";

            var docHead = document.head.outerHTML;
            var printContents = document.getElementById(div).outerHTML;
            var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=865, height=600, resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

            var newWin = window.open("", "_blank", winAttr);
            var writeDoc = newWin.document;
            writeDoc.open();
            writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
            writeDoc.close();
            newWin.focus();

            angular.element('#apptitle').context.title = otitle;
        }

        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);
    }]);