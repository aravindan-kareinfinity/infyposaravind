    
var app = angular.module('InfinityLab');

app.controller('IntrayController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog','$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout) {
        $scope.SearchRef = restService.SearchReference;
        $scope.filteredtype = '';
        $scope.IntrayList = [];
        $scope.igrid = null;
        $scope.ReferenceTypeList = [{ 'id': 0, 'name': 'ALL' }, { 'id': 1, 'name': 'Supplier' },
            { 'id': 3, 'name': 'Product' }, { 'id': 2, 'name': 'Agent' },
            { 'id': 4, 'name': 'Inward' }, { 'id': 5, 'name': 'Task' }, { 'id': 6, 'name': 'Discount' },
            { 'id': 7, 'name': 'Notification' }, { 'id': 8, 'name': 'Purchase Order' }, { 'id': 9, 'name': 'Brand' },
            { 'id': 10, 'name': 'Attributes' }, { 'id': 11, 'name': 'Barcode Generation' }, { 'id': 12, 'name': 'Sales Order' },
            { 'id': 13, 'name': 'Customer' }, { 'id': 14, 'name': 'SKU Design' }, { 'id': 15, 'name': 'BOM' },
            { 'id': 16, 'name': 'BOM PO' }, { 'id': 17, 'name': 'Transfer' }, { 'id': 18, 'name': 'Ledger' }
        ];
        $scope.Company = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            results.splice(0, 0, { 'id': 0, 'name': 'ALL' });
            $scope.Company = results;
        });

        $scope.StatusList = [{ 'id': -1, 'name': 'ALL' },{ 'id': 1, 'name': 'Open' }, { 'id': 4, 'name': 'Approved' }, { 'id': 3, 'name': 'Rejected' }, { 'id': 2, 'name': 'Closed' }];

        $scope.SearchRequest = {
            referencetype: 0,
            actiontype: 1,
            fromdate: "",
            todate: "",
            lastvisitedid:0
        };

        $scope.Search = function (loadNextPage) {
            if (typeof (loadNextPage) == 'undefined') {
                $scope.SearchRequest.lastvisitedid = 0;
                $scope.IntrayList = [];
            }
            request = {
                "Item": $scope.SearchRequest,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "IntrayService/Search",
                data: srequest
            }).success(function (result) {
                if (typeof (loadNextPage) == 'undefined') {
                    $scope.IntrayList = result;
                }
                else {
                    $.each(result, function (k, v) {
                        var found = $scope.IntrayList.find(function (item) { return item.id == v.id; });
                        if (typeof (found) == 'undefined' || found == null)
                            $scope.IntrayList.push(v);
                    })
                }
                $scope.igrid.UpdateData($scope.IntrayList);
                //$timeout(function () {
                //    $scope.igrid.dataView.setPagingOptions({'pageSize':50});
                //}, 100);
                //getViewportHeight

            }).error(function (result) {
            });
        }

        $scope.typechanged = function () {
            var filteredlist = [];
            $.each($scope.IntrayList, function (k, v) {
                if (typeof ($scope.SearchRequest.referencetype) == 'undefined' ||
                    $scope.SearchRequest.referencetype == 0 ||
                    $scope.SearchRequest.referencetype == v.referencetype) {
                    if (typeof ($scope.SearchRequest.companyid) == 'undefined' ||
                        $scope.SearchRequest.companyid == 0 ||
                        $scope.SearchRequest.companyid == v.companyid) {
                        if (typeof ($scope.SearchRequest.actiontype) == 'undefined' ||
                            $scope.SearchRequest.actiontype == 0 ||
                            $scope.SearchRequest.actiontype == v.actiontype) {
                            filteredlist.push(v); 
                        }
                    }
                }
            });
            $scope.igrid.UpdateData(filteredlist);
        }

        $scope.ViewIntrayList = function (item) {
            if (item.referencetype == 1) {
                $location.url('/supplier?id=' + item.referenceid + '&intray=true');
            }
            else if (item.referencetype == 2) {
                $location.url('/Agent?id=' + item.referenceid + '&intray=true');
            }
            else if (item.referencetype == 3) {
                $location.url('/Product?id=' + item.referenceid + '&intray=true');
            }
            else if (item.referencetype == 8) {
                $location.url('/PurchaseOrder?id=' + item.referenceid + '&intray=true');
            }
            else if (item.referencetype == 9) {
                $location.url('/Brand?id=' + item.referenceid + '&intray=true');
            }
            else if (item.referencetype == 10) {
                $location.url('/ProductReflist?id=' + item.referenceid + '&intray=true');
            }
            else if (item.referencetype == 11) {

                request = {
                    "Item": { 'id': item.referenceid },
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "InboxService/Search",
                    data: srequest
                }).success(function (result) {
                    if (result && result.length == 1) {
                        $location.url('/barcode?lrid=' + result[0].referenceid + '&intray=true');
                    }
                }).error(function (result) {
                });

            }
            else if (item.referencetype == 12) {
                $location.url('/SalesOrder?id=' + item.referenceid+'&intray=true');
            }
            else if (item.referencetype == 13) {
                $location.url('/Customer?id=' + item.referenceid + '&intray=true');
            }
            else if (item.referencetype == 14) {
                $location.url('/skucreation?id=' + item.referenceid + '&intray=true');
            }
            else if (item.referencetype == 15) {
                $location.url('/bommaster?id=' + item.referenceid + '&intray=true');
            }
            else if (item.referencetype == 16) {
                $location.url('/ComponentPO?id=' + item.referenceid + '&intray=true');
            }
            else if (item.referencetype == 18) {
                $location.url('/Ledger?id=' + item.referenceid + '&intray=true');
            }
            //$scope.ReferenceTypeList = [{ 'id': 0, 'name': 'ALL' }, { 'id': 1, 'name': 'Supplier' },
            //{ 'id': 3, 'name': 'Product' }, { 'id': 2, 'name': 'Agent' },
            //{ 'id': 4, 'name': 'Inward' }, { 'id': 5, 'name': 'Task' }, { 'id': 6, 'name': 'Discount' },
            //{ 'id': 7, 'name': 'Notification' }, { 'id': 8, 'name': 'Purchase Order' }, { 'id': 9, 'name': 'Brand' },
            //{ 'id': 10, 'name': 'Attributes' }, { 'id': 11, 'name': 'Barcode Generation' }, { 'id': 12, 'name': 'Sales Order' },
            //{ 'id': 13, 'name': 'Customer' }, { 'id': 14, 'name': 'SKU Design' }, { 'id': 15, 'name': 'BOM' }, { 'id': 16, 'name': 'BOM PO' }
        };
        $scope.ManageIntrayList = function (action, item) {

            var type = 1;
            if (action == "View") {
                $scope.ViewIntrayList(item);
            }
            else {
                if (action == "Close")
                    type = 2;
                else if (action == "Approve")
                    type = 4;
                else if (action == "Reject")
                    type = 3;
                request = {
                    "Item": { id: item.intrayid, actiontype: type, isactive: true },
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "IntrayService/UpdateStatus",
                    data: srequest
                }).success(function (result) {
                    $scope.Search();
                }).error(function (result) {
                });
            }
        };

        var igrid = new InfyGrid('gridContainer', '500px', '100%', true, false, true, 50);
        igrid.options.showHeaderRow = true;

        igrid.AddFormatedColumn('actionon', 'Date', 80, 'dd-MM-yyyy');
        igrid.AddSimpleColumn('typename', 'Type', 150);
        igrid.AddSimpleColumn('displaydetail', 'Description', 400);
       
        //igrid.AddCustomColumn('detail', 'Description', 400, function (row, cell, value, columnDef, dataContext) {
        //    return row.detail.description;
        //});

        igrid.AddSimpleColumn('employeename', 'Created By', 150);
        igrid.AddSimpleColumn('modifieduser', 'Action By', 150);
        igrid.AddReferenceColumn('actiontype', 'Status', 100, $scope.StatusList);

        //name, header, width, value, clickevent
        igrid.columns.push({
            id: name, name: 'Action', field: 'action', width: 240, minWidth: 200, resizable: true, focusable: true, selectable: false,
            formatter: function (row, cell, value, columnDef, dataContext) {
                if (dataContext.referencetype == 4 || dataContext.referencetype == 5 || dataContext.referencetype == 7) {
                    return "<input type='button' class='btn btn-primary btn-xs' value='View'/><input type='button' style='margin-left:10px' class='btn btn-primary btn-xs' value='Close'/>"
                }
                else {
                    return "<input type='button' class='btn btn-primary btn-xs' value='View'/><input type='button' style='margin-left:10px' class='btn btn-primary btn-xs' value='Approve'/><input type='button' style='margin-left:10px' class='btn btn-primary btn-xs' value='Reject'/><input type='button' style='margin-left:10px' class='btn btn-primary btn-xs' value='Close'/>";
                }
                //return "<input type='button' class='btn btn-primary btn-xs' value='View'/><input type='button' style='margin-left:10px' class='btn btn-primary btn-xs' value='Close'/>"

            }, clickevent: function (e, args) {

                $scope.$apply(function () {
                    $scope.ManageIntrayList($(e.target).val(),args.item);
                });
                return true;

            }, buttontext: 'Action', headerCssClass: 'text-center'
        });
        igrid.SubscribeClickEvent();
        igrid.onScroll = function (e, args) {
            if ($scope.igrid != null && args && args != null) {
                var row = args.grid.getCellFromPoint(args.scrollLeft, args.scrollTop).row;
                if ($scope.IntrayList.length < row + 30) {
                    var rows = $scope.igrid.dataView.getItems();
                    if (typeof (rows) != 'undefined' && rows != null && rows.length > 0) {
                        if($scope.SearchRequest.lastvisitedid != rows[rows.length - 1].id)
                            $scope.Search(true);
                    }
                }
            }
        }
        //igrid.onPageInfoChanged = function (e, pagingInfo) {
        //    var isLastPage = pagingInfo.pageNum == pagingInfo.totalPages - 1;
        //    if (isLastPage) {
        //        var rows = $scope.igrid.dataView.getItems();
        //        if (typeof (rows) != 'undefined' && rows != null && rows.length > 0) {
        //            $scope.SearchRequest.lastvisitedid = rows[rows.length - 1].id;
        //            $scope.Search(true);
        //        }
        //    }
        //};
        igrid.Initialize();
        $scope.igrid = igrid;
        //$scope.igrid.dataView.setPagingOptions({ 'pageSize': 50 });
    }]);
