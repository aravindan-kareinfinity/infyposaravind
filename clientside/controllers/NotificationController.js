var app = angular.module('InfinityLab');

app.controller('NotificationsController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.SearchResult = [];
        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.Section = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Section = results;
        });
        $scope.AllLocation = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.AllLocation = results;
        });
        $scope.Location = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.Product = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Product = results;
        });
        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
        });
        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
            var exist = false;
            $.each($scope.Sizes, function (k, v) {
                if (v.id == -2)
                    exist = true;
            });
            if (!exist) {
                $scope.Sizes.push({ id: -2, name: 'Jump' });
                $scope.Sizes.push({ id: -3, name: 'Cut' });
            }

        });
        $scope.Material = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Material = results;
        });
        $scope.Pattern = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Pattern = results;
        });
        $scope.Style = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Style = results;
        });
        $scope.Type = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.Type = results;
        });
        $scope.Colour = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Colour = results;
        });
        $scope.SearchCondition = {
            fromdate: "",
            todate: ""
        }

        $scope.exporttoExcel = function (infygrid, filename) {
            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, filename);
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = filename + ".xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }

        $scope.SearchNotifications = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/GetNotifications",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.NotificationConfig();
                $scope.ReorderConfig();

            }).error(function (result) {
            });

        }

        $scope.NotificationConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '250px', '100%', true, false, true);
            igrid.AddSimpleColumn('code', 'CODE', 150);
            igrid.AddSimpleColumn('fromlocation', 'FROM LOCATION', 250);
            igrid.AddSimpleColumn('tolocation', 'TO LOCATION', 250);
            igrid.AddSimpleColumn('tocompany', 'COMPANY NAME', 250);
            igrid.AddSimpleColumn('qty', 'QTY', 100);
            igrid.AddSimpleColumn('packedon', 'PACKED ON', 100);
            igrid.AddSimpleColumn('packedby', 'PACKED BY', 100);

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult.PendingBundles);

        }

        $scope.ReorderConfig = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var sgrid = new InfyGrid('gridContainerselected', '250px', '100%', true, false, true);
            sgrid.AddReferenceColumn('locationid', 'LOCATION', 100, $scope.Location);

            sgrid.AddSimpleColumn('productname', 'PRODUCT', 100);
            //sgrid.AddReferenceColumn('brandid', 'BRAND', 100, $scope.Brand);
            //sgrid.AddReferenceColumn('sizeid', 'SIZE', 100, $scope.Sizes);
            //sgrid.AddReferenceColumn('colourid', 'COLOUR', 100, $scope.Colour);
            //sgrid.AddReferenceColumn('materialid', 'MATERIAL', 100, $scope.Material);
            //sgrid.AddReferenceColumn('patternid', 'PATTERN', 100, $scope.Pattern);
            //sgrid.AddReferenceColumn('styleid', 'PATTERN', 100, $scope.Style);
            //sgrid.AddReferenceColumn('supplierid', 'PATTERN', 100, $scope.Suppliers);
            //sgrid.AddReferenceColumn('typeid', 'TYPE', 100, $scope.Type);
            sgrid.AddSimpleColumn('title', 'Description', 250);
            sgrid.AddSimpleColumn('expect', 'Reorder Level', 100);
            sgrid.AddSimpleColumn('pricerange', 'Price Range', 100);
            sgrid.AddSimpleColumn('retailstock', 'Showroom', 100);
            sgrid.AddSimpleColumn('otherretailstock', 'Others', 100);
            sgrid.AddSimpleColumn('stock', 'Total', 100);
            sgrid.AddSimpleColumn('status', 'STATUS', 150);

            sgrid.Initialize();
            $scope.sgrid = sgrid;
            $scope.sgrid.UpdateData($scope.SearchResult.Reorders);

        }

    }]);