var app = angular.module('InfinityLab');

app.controller('WOInboxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$state', '$filter', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state, $filter, $timeout) {
        $scope.SearchRef = restService.SearchReference;
        $scope.filteredtype = '';
        $scope.Transports = [];
        $scope.City = [];
        $scope.LRInboxList = [];
        $scope.Suppliers = [];
        $scope.Company = [];
        $scope.Location = [];

        restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.Company.push(v);
            });
        });



        $scope.CreateJobStage = function (inward) {

            var selectedRows = $scope.igrid.GetSelectedRows();
            if (selectedRows.length != 1) return;

            if (inward) {
                $location.url('/joborder?workorderid=' + selectedRows[0].mfgworkorderid +
                    '&joborderid=' + selectedRows[0].mfgjoborderid +
                    '&inwardjob=' + true);
                return;
            }


            var i = $scope.igrid.grid.getData().getLength();
            var availablestages = [];
            while (i--) {
                var item = $scope.igrid.grid.getDataItem(i);
                if (item.mfgjoborderid > 0 && item.mfgworkorderid == selectedRows[0].mfgworkorderid) {
                    availablestages.push(item.stageid);
                }
            }

            restService.Get('MfgworkfloworchestrationService/FindStageList/' + selectedRows[0].mfgworkorderid, true).then(function (results) {

                var dialog = ngDialog.open({
                    template: 'ChooseStage',
                    data: {
                        'StageList': results,
                        'AvailableStages': availablestages,
                        'inward': inward,
                        'scope': $scope
                    },
                    width: '500px',
                    controller: ['$scope', '$timeout', '$rootScope', function ($scope, $timeout, $rootScope) {
                        $scope.StageList = $scope.ngDialogData.StageList.stages;
                        $scope.AvailableStages = $scope.ngDialogData.AvailableStages;
                        $scope.inward = $scope.ngDialogData.inward;
                        $scope.stageid = $scope.StageList[0].index;
                        $scope.Selected = function () {

                            var stage = $scope.StageList.find(function (item) {
                                return $scope.stageid == item.index;
                            });

                            if (stage &&
                                $scope.AvailableStages.indexOf($scope.stageid) >= 0) {
                                $rootScope.Confirm('Exist', "Selected Stage (" + stage.name + ') already initiated', function () {
                                    $scope.closeThisDialog({ 'stageid': $scope.stageid, 'corestageid': stage.corestageid });
                                });
                                return;
                            } else if (stage && stage != null) {
                                $scope.closeThisDialog({ 'stageid': $scope.stageid, 'corestageid': stage.corestageid });
                            }

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
                                '&corestageid=' + data.value.corestageid +
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
            { 'id': 3, 'name': 'Work Order Progress' },
            { 'id': 4, 'name': 'Job Work Progress' },
            { 'id': 5, 'name': 'Product Delay for GRN' },
            { 'id': 6, 'name': 'Product Ready for GRN' },
            { 'id': 7, 'name': 'Product Received' },
            { 'id': 8, 'name': 'GRN Received' },
            { 'id': 9, 'name': 'Completed' }
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

        $scope.customerchanged = function () {
            var ftype = $scope.Customers.find(function (item) {
                return $scope.SearchRequest.customerid == item.id;
            });
            if (ftype != null && ftype.id > 0)
                $scope.filteredtype = ftype.name;
            else
                $scope.filteredtype = '';
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
                $scope.igrid.UpdateData($scope.ProcessResult(result));
                $scope.igrid.dataView.refresh();
            }).error(function (result) {
            });
        }

        $scope.CurrentSearchResult = [];
        $scope.ProcessResult = function (result) {
            $.each(result, function (k, v) {
                v.indent = v.mfgjoborderid > 0 ? 1 : 0;
                v.show = !v.mfgjoborderid > 0;
                v.collapsed = true;
            });
            result.sort(function (a, b) {
                if (a.mfgworkorderid == b.mfgworkorderid) {
                    if (a.mfgjoborderid < b.mfgjoborderid) { return -1; }
                    if (a.mfgjoborderid > b.mfgjoborderid) { return 1; }
                    return 0;
                }

                if (a.mfgworkorderid > b.mfgworkorderid) { return -1; }
                if (a.mfgworkorderid < b.mfgworkorderid) { return 1; }
                return 0;
            })
            $scope.CurrentSearchResult = result;
            return result;
        }

        $scope.GetSelecteItemObject = function (type) {
            var seletedRows = $scope.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                if ((type == null || v.inboxtype == type) && (typeof (statusid) == 'undefined' || statusid == null || v.statusid == statusid) && (typeof (modeofdelivery) == 'undefined' || v.modeofdelivery == modeofdelivery))
                    retList.push(v);
            });
            return retList;
        };

        $scope.GetSelecteItems = function (type, statusid, modeofdelivery) {
            var seletedRows = $scope.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                if ((type == null || v.inboxtype == type) && (typeof (statusid) == 'undefined' || statusid == null || v.statusid == statusid) && (typeof (modeofdelivery) == 'undefined' || v.modeofdelivery == modeofdelivery))
                    retList.push(v.referenceid);
            });
            return retList;
        };

        $scope.WorkOrderGRN = function () {
            var seletedRows = $scope.igrid.GetSelectedRows();
            if (seletedRows.length == 1) {
                if (seletedRows[0].isbomprocess) {
                    $rootScope.ShowAlert('InBox', "Selected workorder only supports BOM Process.");
                    return;
                }

                if (seletedRows[0].inqty == 0 && seletedRows[0].outqty == 0) {
                    $rootScope.ShowAlert('InBox', "Selected workorder not processed Packing stage.");
                    return;
                }

                if (seletedRows[0].inqty >= seletedRows[0].outqty) {
                    $rootScope.ShowAlert('InBox', "Already GRN received against this Work order.");
                    return;
                }

                $location.url('/wogrn?id=' + seletedRows[0].mfgworkorderid);
            }
            else {
                $rootScope.ShowAlert('InBox', "Please select work order to create grn");
            }
        };

        $scope.BOMOutward = function () {
            var seletedRows = $scope.igrid.GetSelectedRows();
            if (seletedRows.length == 1) {
                $location.url('/BOMConsumption?workorderid=' + seletedRows[0].mfgworkorderid + '&joborderid=' + seletedRows[0].mfgjoborderid + '&isoutward=true');
            }
            else {
                $rootScope.ShowAlert('InBox', "Please Select WorkOrder...");
            }
        };

        $scope.BOMConsume = function (isoutward) {
            var seletedRows = $scope.igrid.GetSelectedRows();
            if (seletedRows.length == 1) {
                $location.url('/BOMConsumption?workorderid=' + seletedRows[0].mfgworkorderid + '&joborderid=' + seletedRows[0].mfgjoborderid + '&isconsumption=true');
            }
            else {
                $rootScope.ShowAlert('InBox', "Please Select WorkOrder...");
            }
        };

        $scope.BOMRequest = function () {
            var selectedRows = $scope.igrid.GetSelectedRows();
            if (selectedRows.length == 1) {


                restService.Get('MfgworkfloworchestrationService/FindStageList/' + selectedRows[0].mfgworkorderid, true).then(function (results) {

                    var dialog = ngDialog.open({
                        template: 'BOMRequest',
                        data: {
                            'StageList': results,
                            'scope': $scope,
                            'row': selectedRows[0]
                        },
                        width: '500px',
                        controller: ['$scope', '$timeout', '$rootScope', function ($scope, $timeout, $rootScope) {
                            $scope.Modes = [
                                { 'id': 1, 'name': 'BOM Request' },
                                { 'id': 2, 'name': 'JOBORDER Request' },
                                { 'id': 3, 'name': 'WORKORDER Request' },
                                { 'id': 4, 'name': 'WORKORDER DC' },
                                { 'id': 5, 'name': 'JOBORDER DC' }
                            ];
                            $scope.StageList = $scope.ngDialogData.StageList.stages;
                            $scope.row = $scope.ngDialogData.row;
                            $scope.stageid = $scope.StageList[0].index;
                            $scope.modechanged = function () {
                                if ($scope.mode != 1) {
                                    $scope.stageid = $scope.row.stageid;
                                }
                            }
                            $scope.Selected = function () {

                                var stage = $scope.StageList.find(function (item) {
                                    return $scope.stageid == item.index;
                                });

                                if (stage && stage != null) {
                                    $scope.closeThisDialog({
                                        'stageid': $scope.stageid,
                                        'corestageid': stage.corestageid,
                                        'mfgstageid': stage.mfgstageid,
                                        'mode': $scope.mode,
                                    });
                                }
                            };
                        }],
                        className: 'ngdialog-theme-plain'
                    });
                    dialog.closePromise.then(function (data) {
                        {
                            if (typeof (data.value) != 'undefined' && data.value != null &&
                                data.value != '$escape' && data.value != '$document') {

                                $location.url('/BOMRequest?workflowid=' + results.id +
                                    '&workorderid=' + selectedRows[0].mfgworkorderid +
                                    '&joborderid=' + selectedRows[0].mfgjoborderid +
                                    '&stageid=' + data.value.stageid +
                                    '&mode=' + data.value.mode +
                                    '&mfgstageid=' + data.value.mfgstageid +
                                    '&corestageid=' + data.value.corestageid);

                            }
                        }
                    });

                });

            }
            else {
                $rootScope.ShowAlert('InBox', "Please Select WorkOrder...");
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
                $scope.igrid.UpdateData($scope.ProcessResult(result));
                $scope.igrid.dataView.refresh();
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

        $scope.ChangeStatus = function () {

            var items = $scope.igrid.GetSelectedRows();
            if (items.length == 0)
                return;

            var dialog = ngDialog.open({
                template: 'ChooseStatus',
                data: {
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', '$rootScope', function ($scope, $timeout, $rootScope) {
                    $scope.StatusList = [
                        { 'id': 1, 'name': 'Created' },
                        { 'id': 2, 'name': 'Material Issued' },
                        { 'id': 3, 'name': 'Work Order Progress' },
                        { 'id': 4, 'name': 'Job Work Progress' },
                        { 'id': 5, 'name': 'Product Delay for GRN' },
                        { 'id': 6, 'name': 'Product Ready for GRN' },
                        { 'id': 7, 'name': 'Product Received' },
                        { 'id': 8, 'name': 'GRN Received' },
                        { 'id': 9, 'name': 'Completed' }
                    ];
                    $scope.Selected = function () {
                        if ($scope.statusid && $scope.statusid != null &&
                            $scope.statusid != '') {
                            var status = $scope.StatusList.find(function (item) {
                                return $scope.statusid == item.id;
                            });
                            if (status && status != null)
                                $scope.closeThisDialog(status);
                        }
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null &&
                        data.value != '$escape' && data.value != '$document') {
                        var action = data.value;
                        var inboxitems = [];
                        $.each(items, function (k, v) {
                            var inboxstatus = { 'id': v.id, 'hasclosed': false };
                            inboxstatus.statusname = action.name;
                            inboxstatus.statusid = action.id;
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
                                var status = result.find(function (item) {
                                    return item.id == v.id;
                                });
                                if (status && status != null)
                                    v.statusid = status.statusid;
                            });
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.Customers = null;
        $scope.ConfigGrid = function () {

            if (typeof ($rootScope.UserInfo) == 'undefined' ||
                typeof ($rootScope.UserInfo.appsettings) == 'undefined' ||
                $rootScope.UserInfo.appsettings == null) {
                $timeout(function () {
                    $scope.ConfigGrid();
                }, 50)
                return;
            }
            else {
                if ($scope.Customers == null) {
                    $scope.Customers = restService.Get('ReferencelistService/RETAILCUSTOMERLEDGER', true).then(function (results) {
                        $scope.Customers = results;
                    });
                }
            }
            $scope.ismulticompany = $rootScope.UserInfo.appsettings.system_multicompany;
            $scope.ismultilocationentry = $rootScope.UserInfo.appsettings.system_showroompurchase;
            $scope.IsSampleBased = $rootScope.UserInfo.appsettings.mfg_mode == 6;
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 25);
            igrid.options.showHeaderRow = true;
            igrid.AddFormatrerColumn('grouping', '', 40, function (row, cell, value, columnDef, dataContext) {
                var spacer = "<span style='display:inline-block;height:1px;width:8px'></span>";
                if (dataContext["mfgjoborderid"] == 0) {
                    if (dataContext.collapsed) {
                        return spacer + " <span class='infygridtoggle expand'></span>&nbsp;";
                    } else {
                        return spacer + " <span class='infygridtoggle collapse'></span>&nbsp;";
                    }
                } else {
                    return spacer + " <span class='infygridtoggle'></span>&nbsp;";
                }
            });

            //igrid.AddCustomColumn('modeofdelivery', '', 40, function (item) {
            //    if (item.inboxtype == 1 && item.mode == 'External') {
            //        return "<img src='clientside/dist/img/external-issued.png' class = 'igridicon' />";
            //    }
            //    else if (item.inboxtype == 1 && item.mode == 'Internal') {
            //        return "<img src='clientside/dist/img/Internal-issued.png' class = 'igridicon' />";
            //    }
            //    else if (item.inboxtype == 2 && item.mode == 'Internal') {
            //        return "<img src='clientside/dist/img/Internal-received.png' class = 'igridicon' />";
            //    }
            //    else {
            //        return "<img src='clientside/dist/img/external-received.png' class = 'igridicon' />";
            //    }
            //});

            if ($scope.ismulticompany)
                igrid.AddReferenceColumn('companyid', 'Company', 100, $scope.Company);
            if ($scope.ismultilocationentry)
                igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.Location);

            igrid.AddSimpleColumn('workorderno', 'WO.No', 70);
            igrid.AddSimpleColumn('joborderno', 'JO.No', 70);
            igrid.AddSimpleColumn('jobcardno', 'JB.No', 70);
            igrid.AddSimpleColumn('lotno', 'Lot.No', 70);

            igrid.AddCustomColumn('orderdate', 'Date', 80, function (item) {
                if (item.inboxtype == 1 || item.inboxtype == 2)
                    return $filter('date')(item.workorderdate, 'dd-MMM-yyyy');
                else
                    return $filter('date')(item.joborderdate, 'dd-MMM-yyyy');
            });

            igrid.AddCustomColumn('expdate', 'Due Date', 80, function (item) {
                if (item.statusid == 9) return '';
                if (item.inboxtype == 1 || item.inboxtype == 2) {
                    if (new Date(item.woexpdate) < new Date())
                        return $filter('date')(item.woexpdate, 'dd-MMM-yyyy');
                    return '';
                }
                else {
                    if (new Date(item.joexpdate) < new Date())
                        return $filter('date')(item.joexpdate, 'dd-MMM-yyyy');
                    return '';
                }
            });

            if ($scope.IsSampleBased) {
                igrid.AddSimpleColumn('customername', 'Customer', 150);
            }
            else {
                igrid.AddSimpleColumn('suppliername', 'Supplier', 150);
            }
            igrid.AddSimpleColumn('jobname', 'Stage', 150);

            igrid.AddCustomColumn('status', 'Status', 70, function (item) {
                return $scope.SearchRef(item.statusid, $scope.StatusList);
                //if (item.inboxtype == 1 || item.inboxtype == 2)
                //    return $scope.SearchRef(item.statusid, $scope.StatusList);
                //else
                //    return $scope.SearchRef(item.workorderstatusid, $scope.StatusList);
            });

            if ($scope.IsSampleBased) {
                var column = igrid.AddCustomColumn('sampleentryno', 'S.Entry No', 100, function (item, col) {
                    if (item.mfglabel == undefined || item.mfglabel == null) return '';
                    return item.mfglabel.entryno;

                });
                var column = igrid.AddCustomColumn('designno', 'S.Design No', 100, function (item, col) {
                    if (item.mfglabel == undefined || item.mfglabel == null) return '';
                    return item.mfglabel.designno;

                });
                var column = igrid.AddCustomColumn('samplename', 'S.Label Name', 100, function (item, col) {
                    if (item.mfglabel == undefined || item.mfglabel == null) return '';
                    return item.mfglabel.samplename;
                });
                var column = igrid.AddCustomColumn('division', 'S.Division', 100, function (item, col) {
                    if (item.mfglabel == undefined || item.mfglabel == null) return '';
                    return item.mfglabel.division;
                });
                var column = igrid.AddCustomColumn('merchant', 'Merchant', 100, function (item, col) {
                    if (item.mfglabel == undefined || item.mfglabel == null) return '';
                    return item.mfglabel.merchant;

                });
                var column = igrid.AddCustomColumn('marketing', 'Marketing', 100, function (item, col) {
                    if (item.mfglabel == undefined || item.mfglabel == null) return '';
                    return item.mfglabel.marketing;
                });
                var column = igrid.AddCustomColumn('sono', 'SO.No', 100, function (item, col) {
                    if (item.mfglabel == undefined || item.mfglabel == null) return '';
                    return item.mfglabel.sono;
                });
                var column = igrid.AddCustomColumn('sodue', 'SO.Due', 50, function (item, col) {
                    if (item.mfglabel == undefined || item.mfglabel == null) return '';
                    return item.mfglabel.duedays;
                });
                var column = igrid.AddCustomColumn('soqty', 'SO.Qty', 50, function (item, col) {
                    if (item.mfglabel == undefined || item.mfglabel == null) return '';
                    return item.mfglabel.soqty;
                });
                //var column = igrid.AddCustomColumn('sodate', 'SO Date', 120, function (item, col) {
                //    if (item.mfglabel == undefined || item.mfglabel == null) return '';
                //    return $filter('date')(item.sodate, 'dd-MM-yyyy hh:mm a');
                //});
            }
            else {
                igrid.AddSimpleColumn('product', 'Product', 150);
                igrid.AddSimpleColumn('design', 'Design', 150).commafilter = true;
            }
            igrid.AddFormatedColumn('fabricqty', 'Fabric', 80, 'N2', 2);
            igrid.AddFormatedColumn('expqty', 'Exp Qty', 80, 'N2', 2);
            igrid.AddFormatedColumn('outqty', 'Out Qty', 80, 'N2', 2);
            igrid.AddFormatedColumn('inqty', 'In Qty', 80, 'N2', 2);
            igrid.AddFormatedColumn('secondqty', 'Seconds Qty', 80, 'N2', 2);
            igrid.AddFormatedColumn('wasteqty', 'Waste Qty', 80, 'N2', 2);
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
                    }
                });
            });

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            //$scope.igrid.filter = function (item) {
            //    //alert(JSON.stringify(item));
            //    return item.show;
            //}

            $scope.igrid.filter = function (item, args) {
                if (!item.show) return false;

                for (var columnId in args.columnFilters) {
                    if (columnId !== undefined && args.columnFilters[columnId] !== "") {
                        var cindex = args.grid.getColumnIndex(columnId);
                        var c = args.grid.getColumns()[cindex];

                        if (typeof (c.commafilter) != 'undefined' && c.commafilter) {
                            if (item[c.field] == null)
                                return false;
                            var listofvalues = args.columnFilters[columnId].split(',');
                            var exist = false;
                            for (var lv = 0; !exist && lv < listofvalues.length; lv++) {
                                if (item[c.field].toString().toLowerCase().startsWith(listofvalues[lv].toLowerCase())) {
                                    exist = true;
                                }
                                if (listofvalues[lv].toString().toLowerCase().startsWith('%') &&
                                    item[c.field].toString().toLowerCase().indexOf(listofvalues[lv].toLowerCase().substr(1)) >= 0)
                                    exist = true;
                            }
                            if (!exist)
                                return false;
                        }
                        else if (typeof (args.columnTypes[columnId]) != 'undefined' && args.columnTypes[columnId] == 'number') {
                            if (args.columnFilters[columnId].length > 0) {
                                var operator = args.columnFilters[columnId].charAt(0);
                                if (operator == '>' || operator == '<' || operator == '=') {
                                    var d = parseFloat(args.columnFilters[columnId].substr(1));
                                    switch (args.columnFilters[columnId].charAt(0)) {
                                        case '>':
                                            return item[c.field] > d;
                                            break;
                                        case '<':
                                            return item[c.field] < d;
                                            break;
                                        case '=':
                                            return item[c.field] == d;
                                            break;
                                    }
                                }
                                else {
                                    var d = parseFloat(args.columnFilters[columnId]);
                                    if (typeof (item[c.field]) != 'undefined') {
                                        return item[c.field] == d;
                                    }
                                    var fvalue = c.formatter(0, cindex, item[c.field], c, item);
                                    return fvalue == d;
                                }
                                //return item[c.field] != null && item[c.field].toString().indexOf(args.columnFilters[columnId]) > 0;
                            }
                        }
                        else if (typeof (args.columnTypes[columnId]) != 'undefined' && args.columnTypes[columnId] == 'reference') {
                            //if (item[c.field] == null)
                            //    return false;

                            var fvalue = c.formatter(0, cindex, item[c.field], c, item);
                            if (typeof (fvalue) == 'undefined' || fvalue == null ||
                                !fvalue.toString().toLowerCase().startsWith(args.columnFilters[columnId].toLowerCase())) {
                                if (!args.columnFilters[columnId].startsWith('%') ||
                                    fvalue.toString().toLowerCase().indexOf(args.columnFilters[columnId].toLowerCase().substr(1)) < 0) {
                                    return false;
                                }
                            }
                        }
                        else {

                            var fvalue = typeof (item[c.field]) != 'undefined' ? item[c.field] : c.formatter(0, cindex, item[c.field], c, item);
                            if (fvalue == null ||
                                !fvalue.toString().toLowerCase().startsWith(args.columnFilters[columnId].toLowerCase())) {
                                if (!args.columnFilters[columnId].startsWith('%') ||
                                    fvalue.toString().toLowerCase().indexOf(args.columnFilters[columnId].toLowerCase().substr(1)) < 0) {
                                    return false;
                                }
                            }
                            return true;
                        }
                    }
                }
                return true;
            }

            igrid.grid.onClick.subscribe(function (e, args) {
                if (args.cell == 1) {
                    var item = $scope.igrid.dataView.getItem(args.row);
                    if (item) {
                        item.collapsed = !item.collapsed;
                        $.each($scope.CurrentSearchResult, function (k, v) {
                            if (v.mfgworkorderid == item.mfgworkorderid && v.id != item.id) {
                                v.show = !item.collapsed;
                            }
                        });

                        $timeout(function () {
                            $scope.igrid.referesh();
                        }, 100);
                    }
                    e.stopImmediatePropagation();
                }
            });

            $scope.PreLoad();
        }

        $timeout(function () {
            $scope.ConfigGrid();
        }, 10)

    }]);
