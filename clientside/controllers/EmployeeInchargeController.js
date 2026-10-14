var app = angular.module('InfinityLab');

app.controller('EmployeeInchargeController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout', '$stateParams', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout, $stateParams, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;
        $scope.InchargeTypes = [{ 'id': 'PRD', 'name': 'Product' },
        { 'id': 'SEC', 'name': 'Section' },
        { 'id': 'EMP', 'name': 'Manager' }];

        $scope.IncentiveTypes = [{ 'id': '%', 'name': '%' },
        { 'id': 'V', 'name': 'Value' }];

        $scope.CurrentIncharge = { 'id': '', name: 'Select' };
        $scope.ReportPage = false;
        $scope.InchargeList = [];
        $scope.CurrentInchargeChanged = function () {
            $scope.CurrentIncharge = $scope.InchargeTypes.find(function (item) {
                return item.id == $scope.inchargetype;
            });
            if ($scope.CurrentIncharge == undefined ||
                $scope.CurrentIncharge == null) {
                $scope.CurrentIncharge = { 'id': '', name: 'Select' };
            }
            else {
                switch ($scope.CurrentIncharge.id) {
                    case 'PRD':
                        $scope.InchargeList = $scope.ProductList;
                        break;
                    case 'SEC':
                        $scope.InchargeList = $scope.SectionList;
                        break;
                    case 'EMP':
                        $scope.InchargeList = $scope.EmployeeList;
                        break;
                }
            }
        }

        restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.EmployeeList = results;
            $scope.EmployeeGridConfig(results)
        });

        $scope.LocationList = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.LocationList = results;
        });

        $scope.ProductList = restService.Get('ReferencelistService/LIST-PRODUCT', true).then(function (results) {
            $scope.ProductList = results;
        });

        $scope.SectionList = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.SectionList = results;
        });

        $scope.GenerateReport = function () {
            if (!$scope.SearchCondition) {
                $scope.SearchCondition = { sfrom: new Date(), sto: new Date(), reportid: 4, budgetids: [], managerids: [], productids: [] };
            }
            $scope.SearchCondition.reportid = 4;
            $scope.Report();
        };

        $scope.Save = function () {
            var request = {
                "items": $scope.CurrentList,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "EmployeeinchargesService/SaveAll",
                data: srequest
            }).success(function (result) {

                $scope.CurrentList = result;
                $scope.ModifiedCount=0;
                $.each($scope.CurrentList, function (k, v) { v.sno = k + 1; });
                $scope.pgrid.UpdateData($scope.CurrentList, '_id');
                $scope.setFocusinchargeid();

            }).error(function (result) {
            });
        }


        $scope.Report = function () {

            if (!$scope.SearchCondition) {
                $scope.SearchCondition = { sfrom: new Date(), sto: new Date(), reportid: 1, budgetids: [], managerids: [], productids: [] };
            }

            var request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            var reportUrl = ($scope.SearchCondition.reportid === 4) ? "EmployeeinchargesService/ReportPrdBillSales" : "EmployeeinchargesService/Report";

            $http({
                method: 'POST',
                url: reportUrl,
                data: srequest
            }).success(function (result) {
                $scope.ShowReport(result);
            }).error(function (result) {
            });

        }

        $scope.HasConfig = false;

        $scope.ShowReport = function (reportitems) {

            if ($scope.rgrid != null) {
                $scope.rgrid.grid.destroy();
                $('#rptContainer').empty();
            }


            var igrid = new InfyGrid('rptContainer', '425px', '100%', true, true, true, 0);
            
                igrid.AddSimpleColumn('employeename', 'Manager', 140);
                igrid.AddSimpleColumn('productname', 'Product', 160);
                igrid.AddSimpleColumn('sectionname', 'Section', 120);
                igrid.AddFormatedColumn('billedqty', 'Billed Qty', 90, 'N2', 2).total = true;
                igrid.AddFormatedColumn('taxable', 'Taxable', 90, 'N2', 2).total = true;
                igrid.AddFormatedColumn('netcost', 'Net cost', 90, 'N2', 2).total = true;
                igrid.AddFormatedColumn('basecost', 'Base cost', 90, 'N2', 2).total = true;
            

            igrid.options.showHeaderRow = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.rgrid = igrid;
            $scope.rgrid.UpdateData(reportitems, '_id');
        }


        $scope.igrid = null;
        $scope.pgrid = null;
        $scope.CurrentEmployee = { 'name':'Select','id':0 };
        $scope.TotalEmployees = [];
        $scope.EmployeeGridConfig = function (result) {
            $scope.ModifiedCount = 0;
            $scope.pgrid = null;
            $scope.CurrentEmployee = { 'name': 'Select', 'id': 0 };
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }
            $.each(result, function (k, v) { v.sno = k + 1; });
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, false, false, 0);
            igrid.options.showHeaderRow = true;
            var column = igrid.AddSimpleColumn('sno', 'S.No', 50);
            column.cssClass = 'rightalign';
            column.sortable = true;
            column.total = true;
            column.filtertype = 'number';

            igrid.AddSimpleColumn('name', 'Employee', 250);
            igrid.AddButtonColumn('load', '', 40, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Print' class='btn btn-primary btn-xs'><i class='fa fa-gear' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.options.autoEdit = true;
            igrid.options.showHeaderRow = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result, '_id');

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'load') {
                    $timeout(function () {
                        item.rowindex = cell.row;
                        $scope.ShowProductBudget(item)
                        $scope.$digest();
                    }, 10);
                }
            });
        }

        $scope.AddProduct = function () {
            if ($scope.CurrentEmployee == null || $scope.pgrid == null) {
                $rootScope.Toaster.Error("Please select employee");
                return;
            }
            var exist = $scope.CurrentList.find(function (item) {
                return item.locationid == $scope.locationid &&
                    item.inchargetype == $scope.inchargetype &&
                    item.inchargeid == $scope.inchargeid;
            })
            if (exist && exist != null) {
                $rootScope.Toaster.Error("Already added,duplicate not supported");
                return;
            }
            $scope.CurrentList.push({
                'locationid': $scope.locationid,
                'inchargetype': $scope.inchargetype,
                'incasvalue': $scope.incentivetype != '%',
                'employeeid': $scope.CurrentEmployee.id,
                'inchargeid': $scope.inchargeid,
                'incentivetype': $scope.incentivetype,
                'incentive': $scope.incentivevalue
            });
            $scope.ModifiedCount++;
            $.each($scope.CurrentList, function (k, v) { v.sno = k + 1; });
            $scope.pgrid.UpdateData($scope.CurrentList, '_id');
            $scope.setFocusinchargeid();
        }

        $scope.ModifiedCount = 0;
        $scope.CurrentList = [];
        $scope.ShowProductBudget = function (item) {

            $scope.CurrentEmployee = item;

            var request = {
                "item": { 'employeeid': item.id },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeeinchargesService/Search",
                data: srequest
            }).success(function (result) {
                $scope.CurrentList = result;
                $scope.ModifiedCount = 0;
                if ($scope.pgrid != null) {
                    $scope.pgrid.grid.destroy();
                    $('#prdContainer').empty();
                }
                $.each($scope.CurrentList, function (k, v) { v.sno = k + 1; });
                var igrid = new InfyGrid('prdContainer', '425px', '100%', true, true, true, 50);
                igrid.options.showHeaderRow = true;
                igrid.AddSimpleColumn('sno', 'S.No', 50);
                igrid.AddReferenceColumn('locationid', 'Location', 200, $scope.LocationList);
                igrid.AddReferenceColumn('inchargetype', 'Mode', 80, $scope.InchargeTypes);
                var column = igrid.AddCustomColumn('detail', "Detail", 250, function (item, col) {
                    var searchlist = $scope.ProductList;
                    if (item.inchargetype == 'SEC')
                        searchlist = $scope.SectionList;
                    else if (item.inchargetype == 'EMP')
                        searchlist = $scope.EmployeeList;
                    var exist = searchlist.find(function (sitem) { return sitem.id == item.inchargeid; });
                    if (exist && exist != null)
                        return exist.name;
                    return 'Unknow';
                });

                var column = igrid.AddCustomColumn('value', "Value", 80, function (item, col) {
                    return item.incasvalue ? item.incentive : item.incentive + '%';
                });
                column.cssClass = 'rightalign';
                column.sortable = true;
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    return item.incasvalue ? item.incentive : item.incentive + '%';
                };

                column.setValue = function (item, col, newvalue, callback) {
                    var lastValue = col.getValue(item, col);
                    if (newvalue == undefined || newvalue == null)
                        newvalue = 0;

                    if (newvalue.toString().endsWith('%')) {
                        item.incasvalue = false;
                    }
                    else {
                        item.incasvalue = true;
                    }
                    item.incentive = parseFloat(newvalue.replace('%', ''));
                    if (isNaN(item.incentive))
                        item.incentive = 0;
                    if (item.modified == undefined)
                    $scope.ModifiedCount++;
                    item.modified = true;
                }

                igrid.AddButtonColumn('Cancel', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' title='Cancel' value='Cancel' class='btn btn-danger btn-xs'><i class='fa fa-scissors' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                column.editor = Slick.Editors.Text;
                igrid.options.autoEdit = true;
                igrid.options.showHeaderRow = true;
                igrid.EnableTotal();
                igrid.Initialize();

                igrid.grid.onClick.subscribe(function (e) {
                    var cell = igrid.grid.getCellFromEvent(e);
                    var item = igrid.grid.getDataItem(cell.row);
                    var command = igrid.grid.getColumns()[cell.cell].id;
                    if (command == 'Cancel') {
                        $timeout(function () {
                            $scope.CurrentList.splice($scope.CurrentList.indexOf(item), 1);
                            $scope.pgrid.UpdateData($scope.CurrentList, '_id');
                            $scope.ModifiedCount++;
                        }, 10);
                    };
                    
                });

                $scope.pgrid = igrid;
                $scope.pgrid.UpdateData(result, '_id');
            });
        }

    }]);