var app = angular.module('InfinityLab');

app.controller('BRSController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter', 'ngDialog', 'localStorageService','$state',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter, ngDialog, localStorageService, $state) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;


        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Banks = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.Banks = results;
        });


        $scope.SearchResult = [];


        $scope.ErrorStyle = "success";
        $scope.ErrorMessage = "";


        $scope.SearchCondition = {
            accountname: "",
            fromdate: "",
            todate: "",
            status: ""
        }



        $scope.SearchTransaction = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BankchequetransactionService/BRS",
                data: srequest
            }).success(function (result) {
                $scope.SearchSummary = result;
                $scope.SearchResult = result.transactions;
                $scope.ChequeListConfig();
            }).error(function (result) {
            });
        }

        $scope.SaveBRS = function () {
            var items = [];
            $.each($scope.SearchResult, function (k, v) {
                if (v.changed)
                    items.push(v);
            });

            request = {
                "Item": { 'items': items },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BankchequetransactionService/SaveBRS",
                data: srequest
            }).success(function (results) {
                $.each(results, function (k, v) {
                    var exist = $scope.SearchResult.find(function (item) {
                        return item.refid == v.refid && item.transactiontype == v.transactiontype;
                    });
                    if (exist && exist != null) {
                        exist.id = v.id;
                    }
                });
                $scope.igrid.UpdateData($scope.SearchResult, '_id');
            }).error(function (result) {
            });
        }

        $scope.ChangeStatus = function () {

            var dialog = ngDialog.open({
                template: 'ChooseStatus',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', '$http', 'restService', function ($scope, $timeout, $http, restService) {
                    $scope.Parent = $scope.ngDialogData[0];
                    $scope.StatusList = [
                        { 'id': 0, 'name': 'Remove Status' },
                        { 'id': 1, 'name': 'Bounced' },
                        { 'id': 2, 'name': 'Completed' },
                        { 'id': 3, 'name': 'Cancelled' },
                        { 'id': 4, 'name': 'Rejected' }];
                    $scope.Save = function () {
                        $scope.closeThisDialog({ 'date': $scope.date, 'status': $scope.status });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });

            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {
                        var seletedRows = $scope.igrid.GetSelectedRows();
                        $.each(seletedRows, function (k, v) {
                            v.brsstatus = data.value.status;
                            v.realisationon = data.value.date;
                            if (!v.changed)
                                $scope.ChangeCount += 1;
                            v.changed = true;
                        });
                        $scope.igrid.UpdateData($scope.SearchResult, '_id');
                    }
                }
            });
        }

        $scope.ChangeCount = 0;

        $scope.StatusSearchList = [
            { 'id': -1, 'name': 'All' },
            { 'id': 0, 'name': 'New' },
            { 'id': 1, 'name': 'Bounced' },
            { 'id': 2, 'name': 'Completed' },
            { 'id': 3, 'name': 'Cancelled' },
            { 'id': 4, 'name': 'Rejected' }];

        $scope.StatusList = [
            { 'id': 0, 'name': '' },
            { 'id': 1, 'name': 'Bounced' },
            { 'id': 2, 'name': 'Completed' },
            { 'id': 3, 'name': 'Cancelled' },
            { 'id': 4, 'name': 'Rejected' }];

        $scope.TransactionTypes = [
            { 'id': 0, 'name': 'None' },
            { 'id': 1, 'name': 'Supplier' },
            { 'id': 2, 'name': 'Transport' },
            { 'id': 3, 'name': 'Customer' },
            { 'id': 4, 'name': 'Voucher' },
            { 'id': 5, 'name': 'Agent' },
            { 'id': 6, 'name': 'Voucher Item' }
        ];

        $scope.AllChanged = function () {
            $scope.SearchCondition.supplierpayment = $scope.SearchCondition.alltransation;
            $scope.SearchCondition.transportpayment = $scope.SearchCondition.alltransation;
            $scope.SearchCondition.customerpayment = $scope.SearchCondition.alltransation;
            $scope.SearchCondition.vouchers = $scope.SearchCondition.alltransation;
            $scope.SearchCondition.agentpayment = $scope.SearchCondition.alltransation;
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''

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

        $scope.EditVoucher = function () {
            $timeout(function () {
                var row = $scope.igrid.GetSelectedRows()[0];
                if (row.transactiontype == 4 && row.refid > 0)
                    $state.go('voucher', { 'voucher': { 'id': row.refid } });
            }, 10);
        }

        $scope.ChequeListConfig = function () {
            $scope.ChangeCount = 0;
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 5);
            igrid.options.showHeaderRow = true;
            igrid.AddSerialNo(40);
            igrid.AddFormatedColumn('transactionon', 'DATE', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('description', 'Account', 150);
            igrid.AddSimpleColumn('remark', 'Remarks', 150);

            igrid.AddReferenceColumn('transactiontype', 'Type', 100, $scope.TransactionTypes);
            igrid.AddSimpleColumn('transactioncode', 'Reference', 100);

            igrid.AddFormatedColumn('instrumentdate', 'CHEQUE DATE', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('age', 'Age', 80);
            igrid.AddFormatedColumn('realisationon', 'BRS', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('brsstatus', 'Status', 80, $scope.StatusList);

            var column = igrid.AddCustomColumn('Paid', 'Issued', 80, function (item, col) {
                if (item.issued > 0)
                    return $filter('IndianNumber')(item.issued, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.issued;
            }

            var column = igrid.AddCustomColumn('Received', 'Received', 80, function (item, col) {
                if (item.received > 0)
                    return $filter('IndianNumber')(item.received, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.received;
            }

            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            igrid.dataView.getItemMetadata = function (row) {
                var item = igrid.dataView.getItem(row);
                item.style = '';

                //{ 'id': 0, 'name': 'Progress' },
                //{ 'id': 1, 'name': 'Bounced' },
                //{ 'id': 2, 'name': 'Completed' },
                //{ 'id': 3, 'name': 'Cancelled' },
                //{ 'id': 4, 'name': 'Rejected' }];
                if (item.brsstatus) {
                    if (item.brsstatus == 2) {
                        item.style = 'igridwithgrey';
                        return { cssClasses: 'igridwithgrey' };
                    }
                    else if (item.status != 0) {
                        item.style = 'igridbigrowswithred';
                        return { cssClasses: 'igridbigrowswithred' };
                    }
                }
            };

            $scope.igrid.UpdateData($scope.SearchResult, '_id');
        }
        $scope.selectedcount = 0;
        //$timeout(function () {
        //    $scope.setFocusfirst();
        //}, 10);


    }]);

