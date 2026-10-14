
var app = angular.module('InfinityLab');

app.controller('JobWorkInBoxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window',
    'restService', 'ngDialog', '$state', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state, $timeout) {
        $scope.SearchRef = restService.SearchReference;
        $scope.filteredtype = '';
        $scope.Transports = [];
        $scope.City = [];
        $scope.LRInboxList = [];
        $scope.Suppliers = [];
        $scope.Company = [];
        $scope.Location = [];

        $scope.Status = [{ 'id': 0, 'name': 'Pending' }, { 'id': 1, 'name': 'Completed' }, { 'id': 2, 'name': 'All' }];

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
            $scope.ConfigGrid();
            $rootScope.BindCompany($scope, 'SearchCondition.companyid');
        });

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.SearchCondition = {
            status: 0
        }

        $scope.Search = function (status) {
            restService.Get('JobworkbillitemsService/SearchSummary/' + status, true).then(function (results) {
                $scope.igrid.UpdateData(results, '_id');
            });
        }
        $scope.SearchInbox = function () {
            $scope.ShowSearch = true;

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "JobworkbillitemsService/SearchPendingSummary",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.igrid.UpdateData($scope.SearchResult, '_id');
                if (result == 'undefined' || result == null || result.length == 0)
                    $rootScope.Toaster.Info("No Results Found.");
            }).error(function (result) {
            });

        }


        $scope.GetSelecteItemObject = function (type) {
            var seletedRows = this.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                if ((type == null || v.typeid == type) && (typeof (statusid) == 'undefined' || statusid == null || v.statusid == statusid) && (typeof (modeofdelivery) == 'undefined' || v.modeofdelivery == modeofdelivery))
                    retList.push(v);
            });
            return retList;
        };

        $scope.GetSelecteItems = function (type, statusid, modeofdelivery) {
            var seletedRows = this.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                if ((type == null || v.typeid == type) && (typeof (statusid) == 'undefined' || statusid == null || v.statusid == statusid) && (typeof (modeofdelivery) == 'undefined' || v.modeofdelivery == modeofdelivery))
                    retList.push(v.referenceid);
            });
            return retList;
        };

        $scope.GetSelecteItemsstatus = function () {

            var seletedRows = this.igrid.GetSelectedRows();

            var retList = [];
            $.each(seletedRows, function (k, v) {
                retList.push(v);
            });
            return retList;
        };

        $scope.Print = function (id) {
            $rootScope.Download('JobworkbillService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? 0 : id), 'jobworkorder.pdf', 'application/pdf');
            return;

            $timeout(function () {
                var link = document.createElement('a');
                link.download = "jobworkorder.pdf";
                link.href = 'JobworkbillService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? 0 : id);
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }

        $scope.ManageItem = function (action, item) {

            if (action == "Print Job") {
                $scope.Print(item.id);
                return;
            }
            else if (action == "Edit Job") {
                $location.url('/JobWork?id=' + item.id);
            }
            else if (action == "GRN") {
                $location.url('/JobWorkGRN?ids=' + item.id);
            }
            else if (action == "PDF") {
                var seletedRows = this.igrid.GetSelectedRows();
                if (seletedRows.length > 0) {
                    var ids = [];
                    for (var i = 0; i < seletedRows.length; i++) {
                        ids.push(seletedRows[i].id);
                    }
                    $rootScope.PostDownload('JobworkbillService/ImagePDFGeneration', { 'Item': { 'ids': ids } },
                        'jobworkimage.pdf', 'application/pdf');
                }
            }
        }


        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'WAREHOUSE DASHBOARD - INFORMATION'
            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle);
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

        $scope.CreateJobGRN = function () {
            var seletedRows = this.igrid.GetSelectedRows();
            if (seletedRows.length > 0) {
                var supplierid = seletedRows[0].jobworker;
                var ids = [];
                for (var i = 0; i < seletedRows.length; i++) {
                    if (supplierid != seletedRows[i].jobworker) {
                        $rootScope.Toaster.Info("Multiple supplier can't convert to single GRN");
                        return;
                    }
                    ids.push(seletedRows[i].id);
                }
                $location.url('/JobWorkGRN?ids=' + ids.join(','));
            }
            else {
                $location.url('/JobWorkGRN');
            }
        }

        $scope.ConfigGrid = function () {

            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 20);
            igrid.options.showHeaderRow = true;

            igrid.AddReferenceColumn('companyid', 'Company', 100, $scope.Company);
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 80);
            igrid.AddSimpleColumn('jobworker', 'Job Worker', 180);
            igrid.AddFormatedColumn('entrydate', 'Entry Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('expecteddate', 'Due Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('sourcesupplier', 'Supplier', 160);
            igrid.AddSimpleColumn('inproduct', 'Source Product', 120);
            igrid.AddSimpleColumn('outproduct', 'Rec. Product', 120);
            igrid.AddFormatedColumn('oqty', 'Issued Qty', 80, 'N0', 0).total = true;
            igrid.AddFormatedColumn('pqty', 'Pending Qty', 80, 'N0', 0).total = true;

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Edit Job'/> &nbsp;"
            if ($rootScope.UserInfo.p_STOREJOBWORK.print)
                commands += "<input type='button' class='btn btn-blue btn-xs' value='Print Job'/> &nbsp;"
            if ($rootScope.UserInfo.p_STOREJOBWORKGRN.view)
                commands += "<input type='button' class='btn btn-info btn-xs' value='GRN'/> &nbsp;"

            igrid.AddButtonColumn('action', 'ACTION', 200, commands, function (e, args) {
                $scope.$apply(function () {
                    $scope.ManageItem($(e.target).val(), args.item);
                });
            });

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
        }

        $timeout(function () {
            $scope.Search('0');
        }, 10)

    }]);
