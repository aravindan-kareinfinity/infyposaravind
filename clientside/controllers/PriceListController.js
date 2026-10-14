
var app = angular.module('InfinityLab');

app.controller('PriceListController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window',
    'restService', 'ngDialog', '$state', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state, $timeout) {
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;
       
        $scope.CurrentRows = null;
        $scope.DesignList = []
        $scope.CustomerList = []
        $scope.CategoryList = []
        $scope.ConfigGrid = function () {

            var igrid = new InfyGrid('gridDesigns', '100px', '100%', false, true, true, 360);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('designid', 'Design', 120);
            igrid.Initialize();
            $scope.designgrid = igrid;

            igrid = new InfyGrid('gridCustomers', '100px', '100%', false, true, true, 360);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('name', 'Customers', 120);
            igrid.Initialize();
            $scope.customergrid = igrid;

            igrid = new InfyGrid('gridCategory', '100px', '100%', false, true, true, 360);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('name', 'Category', 120);
            igrid.Initialize();
            $scope.categorygrid = igrid;
            $scope.CustomerCategory = restService.Get('ReferencelistService/CUSTOMERCATEGORY', true).then(function (results) {
                $scope.CategoryList = results;
                $scope.categorygrid.UpdateData(results, '_id');
            });
            $scope.Customers = restService.Get('ReferencelistService/RETAILCUSTOMERLEDGER', true).then(function (results) {
                $scope.CustomerList = results;
                $scope.customergrid.UpdateData(results, '_id');
            });
            $scope.Designs = restService.Get('ReferencelistService/PRICEDESIGN', true).then(function (results) {
                $scope.DesignList = results;
                $scope.designgrid.UpdateData(results, '_id');
            });
        }

        $timeout(function () {
            $scope.ConfigGrid();
        }, 10);

        $scope.Search = function () {
            var rows = [];
            var designrows = this.designgrid.GetSelectedRows();
            if (designrows.length == 0) {
                $rootScope.Toaster.Info("Please selecte the designs");
                return;
            }
            var request = { 'designids': [], 'categoryids': [], 'customerids': [] }

            $.each(designrows, function (kk, vv) {
                request.designids.push(vv.id);
            });

            var seletedRows = this.categorygrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                $.each(designrows, function (kk, vv) {
                    rows.push({
                        'display': v.name, 'design': vv.designid, 'designid': vv.id, 'categoryid': v.id,
                        'price': 0, 'oprice': 0, 'customerid': 0
                    });
                });
                request.categoryids.push(v.id);
            });
            var seletedRows = this.customergrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                $.each(designrows, function (kk, vv) {
                    rows.push({
                        'display': v.name, 'design': vv.designid, 'designid': vv.id, 'customerid': v.id,
                        'price': 0, 'oprice': 0, 'categoryid': 0
                    });
                });
                request.customerids.push(v.id);
            });



            $scope.CurrentRows = rows;

            var webrequest = {
                "Item": request,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(webrequest);

            $http({
                method: 'POST',
                url: "CustomerpriceService/SearchAll",
                data: srequest
            }).success(function (result) {
                $.each(result, function (k, v) {
                    var current = rows.find(function (item) {
                        return item.customerid == v.customerid &&
                            item.categoryid == v.categoryid &&
                            item.designid == v.designid;
                    });
                    if (current && current != null) {
                        current.price = v.price;
                        current.oprice = v.price;
                    }
                });
                $scope.displaymode = 'ShowFlatGrid';
                if (request.designids.length < request.categoryids.length + request.customerids.length &&
                    request.designids.length < 10) {
                    $scope.displaymode  = 'ShowDesignGrid';
                } else if (request.designids.length > request.categoryids.length + request.customerids.length &&
                    request.categoryids.length + request.customerids.length < 10) {
                    $scope.displaymode  = 'ShowCustomerGrid';
                }
                $scope.BuildGrid(rows);
            });
        }

        $scope.BuildGrid = function (rows) {
            if ($scope.pricegrid != null) {
                $scope.pricegrid.grid.destroy();
                $('#gridResultContainer').empty();
            }

            if ($scope.displaymode == 'ShowDesignGrid') {
                $scope.ShowDesignGrid(rows);
            } else if ($scope.displaymode == 'ShowCustomerGrid') {
                $scope.ShowCustomerGrid(rows);
            }
            else {
                $scope.ShowFlatGrid(rows);
            }
        }

        $scope.ShowCustomerGrid = function (rows) {
            var rowsource = [];
            var columnsource = [];
            var drows = this.designgrid.GetSelectedRows();
            var valuesource = [];

            var seletedRows = this.categorygrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                columnsource.push({ 'categoryid': v.id, 'customerid': 0, 'display': v.name, 'index': k });
                valuesource.push({ 'categoryid': v.id, 'customerid': 0, 'index': k });
            });
            var seletedRows = this.customergrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                columnsource.push({ 'customerid': v.id, 'categoryid': 0, 'display': v.name, 'index': valuesource.length });
                valuesource.push({ 'customerid': v.id, 'categoryid': 0, 'index': valuesource.length });
            });

            $.each(drows, function (k, v) {
                rowsource.push({
                    'id': v.id, 'display': v.designid, designs: (JSON.parse(JSON.stringify(valuesource)))
                });
            });


            $.each(rows, function (k, v) {
                var row = rowsource.find(function (item) {
                    return item.id == v.designid;
                });
                if (row && row != null) {
                    var designrow = row.designs.find(function (item) {
                        return item.customerid == v.customerid && item.categoryid == v.categoryid;
                    });
                    if (designrow && designrow != null) {
                        designrow.value = v;
                        row['designs_' + designrow.index + '_price'] = v.price;
                    }
                }
            })

            var igrid = new InfyGrid('gridResultContainer', '100px', '100%', false, true, true, 1);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('display', 'Detail', 150);

            $.each(columnsource, function (k, v) {
                var column = igrid.AddColumn(
                    {
                        id: "price_" + v.index, name: v.display, field: "designs_" + v.index + "_price",
                        width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
                column.Design = v;
                column.format = 'N2';
                column.decimal = 2;
                column.cssClass = "rightalign";
                column.formatter = Slick.Formatters.IndianNumberFormatter;
                column.getValue = function (item, col) {
                    var cellresult = item.designs[col.Design.index].value;
                    if (cellresult != null) return cellresult.price;
                    return 0;
                }
            });

            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.grid.onCellChange.subscribe(function (e, args) {
                var rowdata = args.grid.getDataItem(args.row);
                var col = args.grid.getColumns()[args.cell];
                rowdata.designs[col.Design.index].value.price = rowdata[col.field];

                if (rowdata.style != 'igridbigrowswithmark' &&
                    rowdata.designs[col.Design.index].value.oprice != rowdata.designs[col.Design.index].value.price) {
                    rowdata.style = 'igridbigrowswithmark';
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                } else {
                    var changed = false;
                    $.each(rowdata.designs, function (k, v) {
                        if (v.oprice == v.price) {
                            changed = true;
                        }
                    })
                    rowdata.style = changed ? 'igridbigrowswithmark' : '';
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });
            $scope.pricegrid = igrid;
            $scope.pricegrid.UpdateData(rowsource, '_id');
        }

        $scope.ShowDesignGrid = function (rows) {
            var rowsource = [];
            var columnsource = [];
            var drows = this.designgrid.GetSelectedRows();
            var valuesource = [];
            $.each(drows, function (k, v) {
                columnsource.push({ 'id': v.id, 'display': v.designid, 'index': k });
                valuesource.push({ 'id': v.id, 'index': k });
            });
            var seletedRows = this.categorygrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                rowsource.push({
                    'categoryid': v.id, 'customerid': 0, 'display': v.name, designs: (JSON.parse(JSON.stringify(valuesource)))
                });
            });
            var seletedRows = this.customergrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                rowsource.push({ 'categoryid': 0, 'customerid': v.id, 'display': v.name, designs: (JSON.parse(JSON.stringify(valuesource))) });
            });

            $.each(rows, function (k, v) {
                var row = rowsource.find(function (item) {
                    return item.customerid == v.customerid && item.categoryid == v.categoryid;
                });
                if (row && row != null) {
                    var designrow = row.designs.find(function (item) {
                        return item.id == v.designid;
                    });
                    if (designrow && designrow != null) {
                        designrow.value = v;
                        row['designs_' + designrow.index + '_price'] = v.price;
                    }
                }
            })

            var igrid = new InfyGrid('gridResultContainer', '100px', '100%', false, true, true, 1);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('display', 'Detail', 150);

            $.each(columnsource, function (k, v) {
                var column = igrid.AddColumn(
                    {
                        id: "price_" + v.index, name: v.display, field: "designs_" + v.index + "_price",
                        width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
                column.Design = v;
                column.format = 'N2';
                column.decimal = 2;
                column.cssClass = "rightalign";
                column.formatter = Slick.Formatters.IndianNumberFormatter;
                column.getValue = function (item, col) {
                    var cellresult = item.designs[col.Design.index].value;
                    if (cellresult != null) return cellresult.price;
                    return 0;
                }
            });

            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.grid.onCellChange.subscribe(function (e, args) {
                var rowdata = args.grid.getDataItem(args.row);
                var col = args.grid.getColumns()[args.cell];
                rowdata.designs[col.Design.index].value.price = rowdata[col.field];

                if (rowdata.style != 'igridbigrowswithmark' &&
                    rowdata.designs[col.Design.index].value.oprice != rowdata.designs[col.Design.index].value.price) {
                    rowdata.style = 'igridbigrowswithmark';
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                } else {
                    var changed = false;
                    $.each(rowdata.designs, function (k, v) {
                        if (v.oprice == v.price) {
                            changed = true;
                        }
                    })
                    rowdata.style = changed ? 'igridbigrowswithmark' : '';
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });
            $scope.pricegrid = igrid;
            $scope.pricegrid.UpdateData(rowsource, '_id');
        }

        $scope.ShowFlatGrid = function (rows) {
            var igrid = new InfyGrid('gridResultContainer', '100px', '100%', false, true, true, 1);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('display', 'Detail', 150);
            igrid.AddSimpleColumn('design', 'Design', 150);
            igrid.AddFormatedColumn('oprice', 'Current', 100, 'N2', 2);
            var column = igrid.AddColumn(
                {
                    id: "price", name: "Price", field: "price",
                    width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );

            column.format = 'N2';
            column.decimal = 2;
            column.cssClass = "rightalign";
            column.formatter = Slick.Formatters.IndianNumberFormatter;

            igrid.options.autoEdit = true;
            igrid.Initialize();
            //igrid.dataView.getItemMetadata = function (row) {
            //    var rowdata = igrid.grid.getData().getItem(row);
            //    if (rowdata.oprice != rowdata.price)
            //        rowdata.style = 'igridbigrowswithmark';
            //    else
            //        rowdata.style = '';
            //};
            igrid.grid.onCellChange.subscribe(function (e, args) {
                var rowdata = args.grid.getDataItem(args.row);
                if (rowdata.oprice != rowdata.price)
                    rowdata.style = 'igridbigrowswithmark';
                else
                    rowdata.style = '';
                $timeout(function () {
                    args.grid.invalidateRow(args.row);
                    args.grid.render();
                }, 10);
            });
            $scope.pricegrid = igrid;
            $scope.pricegrid.UpdateData(rows, '_id');
        }

        $scope.Save = function () {

            var designrows = this.designgrid.GetSelectedRows();
            //var mfgdesignlist = [];
            var rows = $scope.CurrentRows;
            //$.each($scope.CurrentRows,function (k, v) {
            //    if (v.designid < 0) {
            //        mfgdesignlist.push(v);
            //    }
            //})
            var request = {
                "Items": $scope.CurrentRows,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerpriceService/SaveAll",
                data: srequest
            }).success(function (result) {

                $.each(rows, function (k, v) {
                    if (v.designid < 0) {
                        var mfgdesign = result.find(function (item) {
                            return item.mfgdesignid == (v.designid * -1) &&
                                item.customerid == v.customerid &&
                                item.categorygrid == v.categorygrid;
                        });
                        if (mfgdesign == undefined || mfgdesign == null) {
                            mfgdesign = result.find(function (item) {
                                return item.mfgdesignid == v.designid &&
                                    item.customerid == v.customerid &&
                                    item.categorygrid == v.categorygrid;
                            });
                        }
                        else {
                            v.designid = mfgdesign.designid;
                            v.id = mfgdesign.id;
                            v.mfgdesignid = mfgdesign.mfgdesignid;

                            var mfgesignrow = designrows.find(function (item) {
                                return item.id == (mfgdesign.mfgdesignid*-1);
                            });
                            if (mfgesignrow && mfgesignrow != null) {
                                mfgesignrow.id = mfgdesign.designid;
                                mfgesignrow.mfgdesignid = mfgdesign.mfgdesignid;
                            }
                        }
                        if (mfgdesign && mfgdesign != null) {
                            v.oprice = mfgdesign.price;
                            v.price = mfgdesign.price;
                        }
                    }
                })

                //$.each(mfgdesignlist, function (k, v) {
                //    var customerprice = result.find(function (item) {
                //        return item.mfgdesignid == (v.designid * -1);
                //    });
                //    if (customerprice && customerprice != null) {
                //        var mfgesign = designrows.find(function (item) {
                //            return v.designid == item.id;
                //        });
                //        if (mfgesign && mfgesign != null) {
                //            mfgesign.mfgdesignid = customerprice.mfgdesignid;
                //            mfgesign.id = customerprice.designid;
                //        }
                //    }
                //})

                //$.each(result, function (k, v) {
                //    var current = rows.find(function (item) {
                //        return item.customerid == v.customerid &&
                //            item.categoryid == v.categoryid &&
                //            item.designid == v.designid;
                //    });
                //    if (current && current != null) {
                //        current.price = v.price;
                //        current.oprice = v.price;
                //    }
                //});
                $scope.BuildGrid(rows);
            });
        }

        $scope.CreateCategory = function () {
            $scope.ManageAttribute(function (name) {
                var request = {
                    "Item": { 'name': name, 'type': 'CUSTOMERCATEGORY' },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "ReferencelistService/ManageAttribute",
                    data: srequest
                }).success(function (result) {
                    if (result.id > 0) {
                        $scope.CategoryList.push(result);
                        $scope.categorygrid.UpdateData($scope.CategoryList, '_id');
                    }
                });
            });
        }
        $scope.CreateDesign = function () {
            $scope.ManageAttribute(function (name) {
                var request = {
                    "Item": { 'name': name, 'type': 'PRICEDESIGN' },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "ReferencelistService/ManageAttribute",
                    data: srequest
                }).success(function (result) {
                    if (result.id > 0) {
                        $scope.DesignList.push({
                            'id': result.id, 'designid': result.name
                        });
                        $scope.designgrid.UpdateData($scope.DesignList, '_id');
                    }
                });
            });
        }
        $scope.ManageAttribute = function (callback) {
            var dialog = ngDialog.open({
                template: 'ManageAttribute',
                data: {
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Selected = function () {
                        $scope.closeThisDialog({ 'name': $scope.name });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        callback(data.value.name);
                    }
                }
            });
        }
    }]);
