
var app = angular.module('InfinityLab');

app.controller('EmployeeStickerController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout) {

        $scope.Types = restService.Get('ReferencelistService/Types', true).then(function (results) {
            $scope.Types = results;
        });

        $scope.Divisions = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Divisions = results;
        });

       $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });
       
        $scope.Designations = restService.Get('ReferencelistService/DESIGNATION', true).then(function (results) {
            $scope.Designations = results;
        });

        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
        });
        $scope.SearchRef = restService.SearchReference;
        $scope.SearchResult = [];
        $scope.PrintBarcode = function () {
            var list = [];
            //var selectedrows = $scope.igrid.GetSelectedRows();
            $.each($scope.SearchResult, function (k, v) {
                if (v.fullvalue != 'undefined' && v.fullvalue != null && v.fullvalue == true)
                    list.push({ 'id': v.id, 'code': v.employeecode, 'name': v.name });
            });
            if (list.length == 0) {
                $rootScope.ShowAlert('Salesman barcode', 'Please select the employees');
                return;
            }
            $rootScope.Prompt('Barcode', 'No Of Copies', function (count) {

                var requestitem = { 'copy': count, 'items': list };
                $scope.ChoseReport(requestitem);

            });
        };


        $scope.ChoseReport = function (requestitem) {
            $rootScope.Handlers.SelectPrinter('EMPLOYEECODE', function (printdata) {
                if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                    return;

                requestitem.reportid = printdata.report.id;

                var request = {
                    "Item": requestitem,
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "EmployeeService/PrintBarcode",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result.length > 0) {
                        $scope.Sent2Printer(result, printdata);
                    }
                });
            });
        }

        $scope.Sent2Printer = function (printdocument, printerData) {

            $rootScope.Sent2Printer(printdocument.join(';'), printerData);
            return;

            $.each(printdocument, function (k, v) {

                $rootScope.Sent2Printer(v, printerData);

            })
        }


        $scope.SearchCondition = {
            name: "",
            contactno: "",
            hasleft: ""
        }

        $scope.SelectAllChanged = function () {
            $.each($scope.SearchResult, function (k, v) {
                v.selected = $scope.SearchCondition.SelectAll;
            });
        }
        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeeService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $.each($scope.SearchResult, function (k, v) {
                    v.fullvalue = false;
                });
                $scope.EmployeeConfig();
            }).error(function (result) {
            });

        }

        $scope.EmployeeConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 30);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);

            igrid.SubscribeClickEvent();
            var dcolumn = igrid.AddCustomColumn('fullvalue',
                '<input type="checkbox" style="z-index: auto;margin-right:10px">', 50, function (item, col) {
                    if (item.fullvalue)
                        return '<input type="checkbox" style="z-index: auto;margin-left:15px" checked>';
                    else
                        return '<input type="checkbox" style="z-index: auto;margin-left:15px">';
                });
            dcolumn.sortable = true;

            igrid.AddSimpleColumn('employeecode', 'Emp ID', 100);
            igrid.AddSimpleColumn('name', 'Emp Name', 250);
            igrid.AddReferenceColumn('floorid', 'Floor', 200, $scope.Floors);
            igrid.AddReferenceColumn('sectionid', 'Section', 200, $scope.Divisions);
            igrid.AddReferenceColumn('designationid', 'Designation', 200, $scope.Designations);
            igrid.AddReferenceColumn('locationid', 'Location', 200, $scope.Locations);

            igrid.options.autoEdit = true;
            igrid.Initialize();

            igrid.grid.onHeaderClick.subscribe(function (e, args) {

                if (args.column.id == 'fullvalue' && $(e.target).is(":checkbox")) {
                    if (igrid.grid.getEditorLock().isActive() && !igrid.grid.getEditorLock().commitCurrentEdit()) {
                        e.preventDefault();
                        e.stopImmediatePropagation();
                        return;
                    }

                    if ($(e.target).is(":checked")) {
                        for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                            var item = igrid.grid.getDataItem(i);
                            item.fullvalue = true;
                        }
                    } else {
                        for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                            var item = igrid.grid.getDataItem(i);
                            item.fullvalue = false;
                        }
                    }
                    e.stopPropagation();
                    e.stopImmediatePropagation();
                    $timeout(function () {
                        args.grid.invalidate();
                        args.grid.render();
                    }, 10);
                }

            });

            igrid.grid.onClick.subscribe(function (e, args) {
                if (args.grid.getColumns()[args.cell].id == 'fullvalue') {
                    item = args.grid.getDataItem(args.row);
                    item.fullvalue = !item.fullvalue && $(e.target).val() == 'on';

                    $timeout(function () {
                        args.grid.invalidate();
                        args.grid.render();
                    }, 10);
                }
            });


            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }


        $timeout(function () {
            $scope.setFocusfirst();
            $scope.EmployeeConfig();
        }, 10);
    }]);