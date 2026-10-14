
var app = angular.module('InfinityLab');

app.controller('TailoringInboxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$state', '$filter', '$timeout','$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state, $filter, $timeout, $stateParams) {
        $scope.SearchRef = restService.SearchReference;
        $scope.filteredtype = '';
        $scope.Transports = [];
        $scope.City = [];
        $scope.LRInboxList = [];
        $scope.Employee = [];
        $scope.Company = [];
        $scope.Location = [];
        $scope.SizeGroups = [];
        $scope.Source = {};
        if (!$stateParams.directlaunch) {
             $scope.Source = $stateParams.source;
        }

        $scope.SizeGroups = restService.Get('ReferencelistService/SIZEGROUP', true).then(function (results) {
            $scope.SizeGroups = results;
        });

        restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.Company.push(v);
            });
        });

        $scope.CreateJobStage = function (inward) {

            var selectedRows = this.igrid.GetSelectedRows();
            if (selectedRows.length != 1) return;

            restService.Get('MfgworkfloworchestrationService/FindStageList/' + selectedRows[0].mfgworkorderid, true).then(function (results) {

                var dialog = ngDialog.open({
                    template: 'ChooseStage',
                    data: {
                        'StageList': results,
                        'inward': inward,
                        'scope': $scope
                    },
                    width: '500px',
                    controller: ['$scope', '$timeout', function ($scope, $timeout) {
                        $scope.StageList = $scope.ngDialogData.StageList.stages;
                        $scope.stageid = -1;
                        $scope.inward = $scope.ngDialogData.inward;
                        $timeout(function () {
                            $scope.stageid = 0
                        },10);
                        $scope.Selected = function () {
                            $scope.closeThisDialog({ 'stageid': $scope.stageid });
                        };
                    }],
                    className: 'ngdialog-theme-plain'
                });
                dialog.closePromise.then(function (data) {
                    {
                        if (typeof (data.value) != 'undefined' && data.value != null &&
                            data.value != '$escape' && data.value != '$document') {

                            $location.url('/joborder?workflowid=' + results.id +
                                '&workorderid=' + selectedRows[0].mfgworkorderid +
                                '&stageid=' + data.value.stageid +
                                '&inwardjob=' + inward);
                        }
                    }
                });

            });

        }

        restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.Location.push(v);
            })
        });

        $scope.ItemTypes = [
            { 'id': 0, 'name': 'All' },
            { 'id': 1, 'name': 'Internal Work Order' },
            { 'id': 2, 'name': 'External Work Order' },
            { 'id': 3, 'name': 'Internal Job Order' },
            { 'id': 4, 'name': 'External Job Order' }
        ];

        $scope.StatusList = [
            { 'id': 0, 'name': 'All' },
            { 'id': 1, 'name': 'Created' },
            { 'id': 2, 'name': 'Material Issued' },
            { 'id': 3, 'name': 'Product Delay for GRN' },
            { 'id': 4, 'name': 'Product Ready for GRN' },
            { 'id': 5, 'name': 'GRN Received' },
            { 'id': 6, 'name': 'Product Received' },
            { 'id': 7, 'name': 'Completed' }
        ];

        $scope.typechanged = function () {
            if ($scope.SearchRequest.inboxtype == 6 || $scope.SearchRequest.inboxtype == 7 || $scope.SearchRequest.inboxtype == 0) {
                $scope.filteredtype = '';
            }
            else {
                var ftype = $scope.ItemTypes.find(function (item) {
                    return $scope.SearchRequest.inboxtype == item.id;
                });
                if (ftype != null && ftype.id > 0)
                    $scope.filteredtype = ftype.name;
                else
                    $scope.filteredtype = '';
            }
        };


        $scope.supplierchanged = function () {

            var ftype = $scope.Suppliers.find(function (item) {
                return $scope.SearchRequest.supplierid == item.id;
            });
            if (ftype != null && ftype.id > 0)
                $scope.filteredtype = ftype.name;
            else
                $scope.filteredtype = '';

        };

        $scope.statuschanged = function () {

            var ftype = $scope.StatusList.find(function (item) {
                return $scope.SearchRequest.statusid == item.id;
            });
            if (ftype != null && ftype.id >= 200)
                $scope.filteredtype = ftype.name;
            else
                $scope.filteredtype = '';

        };

        $scope.SearchRequest = {
            inboxtype: 0,
            addedfrom: null,
            supplierid: null,
            statusid: null,
            addedto: null
        };

        restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.Suppliers.push(v);
            })
        });
        restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.City.push(v);
            })
        });

        $scope.Search = function () {
            $scope.LRInboxList = [];
            request = {
                "Item": $scope.SearchRequest,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgworkorderinboxService/Search",
                data: srequest
            }).success(function (result) {
                //alert(result.length);
                $scope.igrid.UpdateData(result);
            }).error(function (result) {
            });
        }

        
        $scope.GetSelecteItemObject = function (type) {
            var seletedRows = this.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                if ((type == null || v.inboxtype == type) && (typeof (statusid) == 'undefined' || statusid == null || v.statusid == statusid) && (typeof (modeofdelivery) == 'undefined' || v.modeofdelivery == modeofdelivery))
                    retList.push(v);
            });
            return retList;
        };

        $scope.GetSelecteItems = function (type, statusid, modeofdelivery) {
            var seletedRows = this.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                if ((type == null || v.inboxtype == type) && (typeof (statusid) == 'undefined' || statusid == null || v.statusid == statusid) && (typeof (modeofdelivery) == 'undefined' || v.modeofdelivery == modeofdelivery))
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

        $scope.WorkOrderGRN = function () {
            var seletedRows = this.igrid.GetSelectedRows();
            if (seletedRows.length == 1) {
                $location.url('/wogrn?id=' + seletedRows[0].mfgworkorderid);
            }
            else {
                $rootScope.ShowAlert('InBox', "Please select work order to create grn");
            }
        };


        $scope.PreLoad = function () {
            request = {
                "Item": {
                    inboxtype: 0,
                    addedfrom: null,
                    addedto: null,
                    showallopen: true
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgworkorderinboxService/Search",
                data: srequest
            }).success(function (result) {
                $scope.igrid.UpdateData(result);
            }).error(function (result) {
            });
        };

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'MFG WORK ORDER DASHBOARD - INFORMATION'
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


        $scope.StatusChange = function (action) {

            var items = $scope.GetSelecteItemsstatus();
            if (items.length == 0)
                return;

            var inboxitems = [];
            $.each(items, function (k, v) {
                var inboxstatus = { 'id': v.id, 'hasclosed': false };
                inboxstatus.statusname = action;

                if (action == "Working") {
                    inboxstatus.statusid = 1;
                }
                else if (action == "Completed") {
                    inboxstatus.statusid = 2;
                    inboxstatus.hasclosed = true;
                }
                else if (action == "Cancel") {
                    inboxstatus.statusid = 3;
                }
                inboxitems.push(inboxstatus);
            });

            request = {
                "Items": inboxitems,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgworkorderinboxService/UpdateStatus",
                data: srequest
            }).success(function (result) {
                $.each(items, function (k, v) {
                    $scope.LRInboxList.splice($scope.LRInboxList.indexOf(v), 1);
                });
            }).error(function (result) {
            });
        };

        $scope.ConfigGrid = function () {

            if (typeof ($rootScope.UserInfo) == 'undefined' ||
                typeof ($rootScope.UserInfo.appsettings) == 'undefined' ||
                $rootScope.UserInfo.appsettings == null) {
                $timeout(function () {
                    $scope.ConfigGrid();
                }, 50)
                return;
            }

            $scope.ismulticompany = $rootScope.UserInfo.appsettings.system_multicompany;
            $scope.ismultilocationentry = $rootScope.UserInfo.appsettings.system_showroompurchase;

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 25);
            igrid.options.showHeaderRow = true;
            igrid.AddCustomColumn('modeofdelivery', '', 40, function (item) {
                if (item.inboxtype == 1 && item.mode == 'External') {
                    return "<img src='clientside/dist/img/external-issued.png' class = 'igridicon' />";
                }
                else if (item.inboxtype == 1 && item.mode == 'Internal') {
                    return "<img src='clientside/dist/img/Internal-issued.png' class = 'igridicon' />";
                }
                else if (item.inboxtype == 2 && item.mode == 'Internal') {
                    return "<img src='clientside/dist/img/Internal-received.png' class = 'igridicon' />";
                }
                else {
                    return "<img src='clientside/dist/img/external-received.png' class = 'igridicon' />";
                }
            });

            if ($scope.ismulticompany)
                igrid.AddReferenceColumn('companyid', 'Company', 100, $scope.Company);
            if ($scope.ismultilocationentry)
                igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.Location);
            igrid.AddCustomColumn('orderdate', 'Date', 80, function (item) {
                if (item.inboxtype == 1 || item.inboxtype == 2)
                    return $filter('date')(item.workorderdate, 'dd-MMM-yyyy');
                else
                    return $filter('date')(item.joborderdate, 'dd-MMM-yyyy');
            });
            igrid.AddCustomColumn('orderno', 'Order No', 80, function (item) {
                if (item.inboxtype == 1 || item.inboxtype == 2)
                    return item.workorderno;
                else
                    return item.joborderno;
            });
            igrid.AddReferenceColumn('supplierid', 'Supplier', 200, $scope.Suppliers);
            igrid.AddCustomColumn('status', 'Status', 70, function (item) {
                if (item.inboxtype == 1 || item.inboxtype == 2)
                    return $scope.SearchRef(item.workorderstatusid, $scope.StatusList);
                else
                    return $scope.SearchRef(item.workorderstatusid, $scope.StatusList);
            });
            igrid.AddSimpleColumn('description', 'Detail', 150);
            igrid.AddSimpleColumn('product', 'Product', 150);
            igrid.AddSimpleColumn('design', 'Design', 150);
            igrid.AddFormatedColumn('outqty', 'Out Qty', 80, 'N2', 2);
            igrid.AddFormatedColumn('expqty', 'Exp Qty', 80, 'N2', 2);
            igrid.AddFormatedColumn('inqty', 'In Qty', 80, 'N2', 2);

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Manage'/>"

            igrid.AddButtonColumn('action', 'ACTION', 175, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Manage') {
                        if (args.item.mfgjoborderid > 0) {
                            $location.url('/joborder?id=' + args.item.mfgjoborderid);
                        }
                        else {
                            $location.url('/workorder?id=' + args.item.mfgworkorderid);
                        }
                        $scope.ManageLRInboxList(args.item);
                    }
                });
            });


            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.PreLoad();
        }

        $timeout(function () {
            $scope.ConfigGrid();
        },10)

    }]);
