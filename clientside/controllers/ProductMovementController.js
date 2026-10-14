
var app = angular.module('InfinityLab');

app.controller('ProductMovementController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter', 'Excel', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, Excel, $timeout) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.reporttype = "PRODUCT";
        $scope.ReportType = [{ 'code': 'PRODUCT', 'name': 'PRODUCT' },
            { 'code': 'BRAND', 'name': 'BRAND' },
            { 'code': 'MATERIAL', 'name': 'MATERIAL' },
            { 'code': 'PATTERN', 'name': 'PATTERN' },
            { 'code': 'STYLE', 'name': 'STYLE' },
            { 'code': 'SIZE', 'name': 'SIZE' },
            { 'code': 'DESIGN', 'name': 'DESIGN' },
            { 'code': 'SUPPLIER', 'name': 'SUPPLIER' }
        ];
        $scope.Supplier = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Supplier = results;
        });

        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
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
        $scope.Section = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Section = results;
        });
        //$scope.labels = ["January", "February", "March", "April", "May", "June", "July"];
        $scope.series = ['Sale Value', 'Sale Qty'];
        //$scope.data = [[65, 59, 80, 81, 56, 55, 40], [28, 48, 40, 19, 86, 27, 90], [18, 28, 46, 49, 66, 37, 30]];

        $scope.datasetOverride = [{ yAxisID: 'y-axis-1' }, { yAxisID: 'y-axis-2' }];
        $scope.options = {
            scales: {
                yAxes: [
                  {
                      id: 'y-axis-1',
                      type: 'linear',
                      display: true,
                      position: 'left'
                  },
                  {
                      id: 'y-axis-2',
                      type: 'linear',
                      display: true,
                      position: 'right'
                  }
                ]
            }
        };


        $scope.SearchCondition = {
            reporttype: 'PRODUCT',
            locationid: 0,
            counterid: 0,
            fromdate: new Date(),
            todate: new Date()
        };
        $scope.ChangeReport = function () {
            $scope.FastMoving = [];
            $scope.SlowMoving = [];
        }

        $scope.FastMoving = [];
        $scope.SlowMoving = [];
        $scope.Search = function () {

            $scope.Product = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchFastmovingItem",
                data: srequest
            }).success(function (result) {
                $scope.FastMoving = result.lstproduct;
                $scope.SlowMoving = result.lstbrand;
                $scope.LoadGraphData();
            }).error(function (result) {
            });
        }

        $scope.LoadGraphData = function()
        {
            $scope.labels = [];
            $scope.data = [];
            var dataset1 = [];
            var dataset2 = [];
            $.each($scope.FastMoving, function (k, v) {
                $scope.labels.push(v.name);
                dataset1.push(v.value);
            });
            $.each($scope.FastMoving, function (k, v) {
                dataset2.push(v.qty);
            });
            $scope.data.push(dataset1);
            $scope.data.push(dataset2);

            $scope.slowlabels = [];
            $scope.slowdata = [];
            var dataset3 = [];
            var dataset4 = [];
            $.each($scope.SlowMoving, function (k, v) {
                $scope.slowlabels.push(v.name);
                dataset3.push(v.value);
            });
            $.each($scope.SlowMoving, function (k, v) {
                dataset4.push(v.qty);
            });
            $scope.slowdata.push(dataset3);
            $scope.slowdata.push(dataset4);

        }
        $scope.printDiv = function (div, title) {

            $timeout(function () {

                var otitle = angular.element('#apptitle').context.title;
                var titlele = angular.element('#apptitle').context.title = title;

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
            }, 100);

        }
        $scope.exporttoExcel = function (tableid, filename) {
            $scope.exportHref = Excel.tableToExcel(tableid, filename);
            $timeout(function () {
                var link = document.createElement('a');
                link.download = filename + ".xls";
                link.href = $scope.exportHref;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);


 }]);