
var app = angular.module('InfinityLab');

app.controller('ProductDesignController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window',
    'restService', 'ngDialog', '$state', '$timeout', '$filter', '$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state, $timeout, $filter, $stateParams) {
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;
        $scope.showproduct = false;
        $scope.showsupplier = true;
        $scope.IsDesignMaster = $stateParams.type == 'DesignMaster';
        $scope.system_ismfgapplication = false;
        $scope.EditMode = false;
        $scope.LoadAppsettings = function () {
            if (typeof ($rootScope.UserInfo) == 'undefined' ||
                typeof ($rootScope.UserInfo.appsettings) == 'undefined' ||
                $rootScope.UserInfo.appsettings == null ||
                typeof ($rootScope.UserInfo.appsettings.system_ismfgapplication) == 'undefined') {
                $timeout(function () {
                    $scope.LoadAppsettings();
                }, 10);
                return;
            }
            $scope.system_ismfgapplication = $rootScope.UserInfo.appsettings.system_ismfgapplication;
            $scope.showsupplier = $scope.system_ismfgapplication == false;
        }

        $scope.LocationList = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.LocationList = results;
        });

        $scope.ColourList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            results.push({ 'id': 0, 'name': 'ALL' });
            $scope.ColourList = results;
            $scope.CurrentColourList = results;
        });

        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
            $scope.CurrentSizeList = results;
        });

        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });

        $scope.SupplierList = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.SupplierList = results;
        });

        $scope.TryAddItem = function () {

            $.confirm({
                title: 'New Design',
                content: 'Do you want to create ' + $scope.newdesign + " design?",
                confirmButton: 'Yes',
                confirmButtonClass: 'btn-info',
                icon: 'fa fa-question-circle',
                animation: 'scale',
                animationClose: 'top',
                opacity: 0.5,
                buttons: {
                    formSubmit: {
                        text: 'Submit',
                        btnClass: 'btn-blue',
                        action: function () {
                            var srequest = JSON.stringify({
                                'Item':
                                {
                                    'productid': $scope.ProductGallery.ProductID,
                                    'supplierid': $scope.ProductGallery.Supplierid,
                                    'designid': $scope.newdesign
                                }
                            });
                            $http({
                                method: 'POST',
                                url: "ProductdesignService/Save",
                                data: srequest
                            }).success(function (result) {
                                if (result && result.id > 0) {
                                    $scope.newdesign = '';
                                    if ($scope.DesignList == undefined || $scope.DesignList == null)
                                        $scope.DesignList = [];
                                    $scope.DesignList.push({
                                        'name': result.designid, 'id': result.id,
                                        'productid': result.productid, 'supplierid': result.supplierid,
                                        'selected': false
                                    });
                                }
                            });
                        }
                    },
                    cancel: function () {
                        //close
                    },
                },

                onContentReady: function () {
                    // bind to events
                    var jc = this;
                    jc.$$formSubmit.trigger('focus');
                    this.$content.find('form').on('submit', function (e) {
                        // if the user submits the form by pressing enter in the field.
                        e.preventDefault();
                        jc.$$formSubmit.trigger('click'); // reference the button and click it
                    });
                }
            });
        }

        $scope.DeleteArray = function (array, item) {
            array.splice(array.indexOf(item), 1);
        }

        $scope.AddColour = function () {
            if ($scope.colourid && $scope.colourid != null) {

                if ($scope.CurrentDesign.colours == undefined ||
                    $scope.CurrentDesign.colours == null)
                    $scope.CurrentDesign.colours = [];

                var exist = $scope.CurrentDesign.colours.find(function (item) {
                    return item.id == $scope.colourid;
                });
                if (exist == undefined || exist == null) {
                    $scope.CurrentDesign.colours.push({ 'id': $scope.colourid, 'name': $scope.SearchRef($scope.colourid, $scope.ColourList) });
                }
            }
        }

        $scope.AddSize = function () {
            if ($scope.sizeid && $scope.sizeid != null) {

                if ($scope.CurrentDesign.detail == undefined ||
                    $scope.CurrentDesign.detail == null)
                    $scope.CurrentDesign.detail = {};

                if ($scope.CurrentDesign.detail.sizes == undefined ||
                    $scope.CurrentDesign.detail.sizes == null)
                    $scope.CurrentDesign.detail.sizes = [];

                if ($scope.CurrentDesign.detail.sizes.indexOf($scope.sizeid) < 0) {
                    $scope.CurrentDesign.detail.sizes.push($scope.sizeid);
                }
            }
        }

        $scope.ShowDesign = function (design) {

            if (($scope.ProductGallery == undefined || $scope.ProductGallery.ProductID == undefined) &&
                ($scope.ProductGallery == undefined || $scope.ProductGallery.Supplierid == undefined)) {
                $rootScope.Toaster.Info("Please select either Product or Supplier or Both");
                return;
            }

            $scope.CurrentDesign = design;
            if ($scope.EditMode) {
                if (design.detail == undefined)
                    design.detail = { 'sizes': [] };

                if (design.detail.sizes == undefined || design.detail.sizes == null)
                    design.detail.sizes = [];

                if (design.colours == undefined)
                    design.colours = [];
            }
            else {
                if ($scope.ProductGallery.ProductID == undefined ||
                    $scope.ProductGallery.ProductID == null || $scope.ProductGallery.ProductID < 0)
                    $scope.showproduct = true;
                else
                    $scope.showproduct = false;

                if ($scope.ProductGallery.Supplierid == undefined ||
                    $scope.ProductGallery.Supplierid == null || $scope.ProductGallery.Supplierid < 0)
                    $scope.showsupplier = true;
                else
                    $scope.showsupplier = false;

                var sizeids = [];
                var colourids = [];
                if (design == undefined || design == null) {
                    $.each($scope.CurrentSizeList, function (k, v) {
                        if (v.selected)
                            sizeids.push(v.id);
                    })
                    $.each($scope.CurrentColourList, function (k, v) {
                        if (v.selected)
                            colourids.push(v.id);
                    })
                }

                var srequest = JSON.stringify({
                    'item':
                    {
                        'productid': $scope.ProductGallery.ProductID,
                        'supplierid': $scope.ProductGallery.Supplierid,
                        'designid': (design && design != null) ? design.designid : null,
                        'allstock': true,
                        'groupsize': true,
                        'includesales': true,
                        'colourids': colourids,
                        'sizeids': sizeids,
                        'fromdate': $scope.EditMode || $scope.SearchCondition == undefined ? '' : $scope.SearchCondition.fromdate,
                        'todate': $scope.EditMode || $scope.SearchCondition == undefined ? '' : $scope.SearchCondition.todate,
                        'pfromdate': $scope.EditMode || $scope.SearchCondition == undefined ? '' : $scope.SearchCondition.pfromdate,
                        'ptodate': $scope.EditMode || $scope.SearchCondition == undefined ? '' : $scope.SearchCondition.ptodate
                    }
                });
                $http({
                    method: 'POST',
                    url: "RetailReportService/SearchDesignwiseStock",
                    data: srequest
                }).success(function (result) {
                    $scope.CurrentDesignwiseStock = result;
                    $scope.limit = 20;
                    var width = (result.sizelist.length + 1) * 80;
                    width += 10;
                    $scope.scrollwidth = { 'width': width + 'px' };
                    $scope.SizeList = result.sizelist;
                    $scope.StockList = [];
                    var id = 0;
                    $.each(result.stock, function (k, v) {

                        if (v.imageid == '' || v.imageid == null || v.imageid == undefined) {
                            var design = $scope.DesignList.find(function (item) {
                                return item.designid == v.designid;
                            });
                            if (design && design != null &&
                                design.colours && design.colours != null) {
                                var designcolour = design.colours.find(function (item) {
                                    return item.id == v.colourid;
                                });
                                if (designcolour && designcolour != null) {
                                    v.imageid = designcolour.image;
                                }
                            }
                        }

                        if (v.imageid && v.imageid != null && v.imageid != '') {
                            v.imageid = 'File/Show/0/' + v.imageid;
                        }
                        else {
                            delete v.imageid;
                        }

                        var x = $scope.StockList.find(function (item) {
                            return item.designid == v.designid;
                        });
                        if (x == undefined || x == null) {
                            x = JSON.parse(JSON.stringify(v));
                            x.id = id++;
                            $scope.StockList.push(x);
                            x.items = [];
                        }
                        cx = JSON.parse(JSON.stringify(v));
                        cx.id = id++;
                        x.items.push(cx);
                    })

                    $scope.ShowDesignStock($scope.StockList);
                });
            }
        }

        $scope.ShowDesignStock = function (stocklist) {
            if ($scope.sgrid && $scope.sgrid != null) {
                $scope.sgrid.grid.destroy();
                $('#gridResultContainer').empty();
            }
            var igrid = new InfyGrid('gridResultContainer', '100px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.options.rowHeight = 60;
            igrid.AddSimpleColumn('designid', 'Design', 100);
            igrid.AddReferenceColumn('supplierid', 'Supplier', 200, $scope.SupplierList);
            igrid.AddReferenceColumn('colourid', 'Colour', 200, $scope.ColourList);
            column = igrid.AddCustomColumn('img_', '', 50, function (item, col) {
                if (item.imageid == undefined ||
                    item.imageid == null) {
                    return '';
                }
                return '<div style="height: 100%;display: flex;align-items: center;justify-content: center;"><img style="width:100%;height:100%" src="' + col.urlprefix + item.image + '"/></div>';
            });
            column.urlprefix = 'File/Show/' + $scope.UserInfo.OrganizationID + '/';
            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'Filter',
                    name: 'Details',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            

            $.each($scope.SizeList, function (k, v) {
                igrid.columns = [];
                var sizecolunm = igrid.AddCustomColumn('sizeid_sl' + k, 'Sale', 50, function (item, col) {
                    return $scope.StockDisplay(item, col.size).bqty;
                });
                sizecolunm.size = v;
                var sizecolunm = igrid.AddCustomColumn('sizeid_sl' + k, 'Stock', 50, function (item, col) {
                    return $scope.StockDisplay(item, col.size).qty;
                });
                sizecolunm.size = v;
                groupColumns.push(
                    {
                        id: 'sizeid_'+k,
                        name: v,
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            });
            igrid.columns = groupColumns;
            igrid.Initialize();
            $scope.sgrid = igrid;
            igrid.UpdateData(stocklist, '_id');
        }

        $scope.AssignImage = function (ditem, item) {
            if (item.imageid) {
                ditem.colour = item.colour;
                ditem.imageid = item.imageid;
            }
            else {
                delete ditem.imageid;
                delete ditem.colour;
            }
        }

        $scope.limit = 20; // initial value for limit
        $scope.loadMore = function (last, inview) {
            if (last && inview) {
                $scope.limit += 10
            }
        }

        $scope.StockDisplay = function (item, size) {
            var st = 0;
            var sl = 0;
            var exist = item.stock.find(function (sitem) {
                return sitem.size == size;
            });
            if (exist && exist != null) {
                return exist;

                //exist.bqty + '/' + exist.qty;
            }
            return { 'bqty': '', 'qty': '' };
        }

        $scope.RemoveImage = function (item, imgitem) {
            item.imagelist.splice(item.imagelist.indexOf(imgitem), 1);
            if (imgitem.id && imgitem.id > 0) {
                if (item.removelist == undefined || item.removelist == null)
                    item.removelist = [];
                item.removelist.push({ 'id': imgitem.id });
            }
        }

        $scope.imageLineuploaded = function (value, scope) {
            scope.colour.image = value[0];
            scope.colour.timageid = null;
        }


        $scope.SyncDesigns = function () {

            var srequest = JSON.stringify({
                'Item':
                {
                    'productid': $scope.ProductGallery ? $scope.ProductGallery.ProductID : 0,
                    'supplierid': $scope.ProductGallery ? $scope.ProductGallery.Supplierid : 0
                }
            });
            $http({
                method: 'POST',
                url: "ProductdesignService/Sync",
                data: srequest
            }).success(function (result) {
                $scope.DesignList = result;
                if ($scope.IsDesignMaster) {
                    $scope.ShowDesignList();
                }
            });
        }

        $scope.ImportDesigns = function () {

            if (($scope.ProductGallery == undefined || $scope.ProductGallery.ProductID == undefined) &&
                ($scope.ProductGallery == undefined || $scope.ProductGallery.Supplierid == undefined)) {
                $rootScope.Toaster.Info("Please select either Product or Supplier or Both");
                return;
            }

            var srequest = JSON.stringify({
                'Item':
                {
                    'productid': $scope.ProductGallery.ProductID,
                    'supplierid': $scope.ProductGallery.Supplierid
                }
            });
            $http({
                method: 'POST',
                url: "ProductdesignService/Import",
                data: srequest
            }).success(function (result) {
                $scope.DesignList = result;
                if ($scope.IsDesignMaster) {
                    $scope.ShowDesignList();
                }
            });
        }


        $scope.MergeDesign = function () {
            if ($scope.igrid == undefined || $scope.igrid == null) return;

            var rows = $scope.igrid.GetSelectedRows();
            if (rows.length == 0) return;

            var ids = [];
            var supplierid = rows[0].supplierid;
            var productid = rows[0].productid;
            $.each(rows, function (k, v) {
                if (supplierid == v.supplierid && productid == v.productid) {
                    ids.push(v.id);
                }
            });
            if (ids.length != rows.length) {
                $rootScope.Toaster.Error("Mulitple supplier/product can't merge");
                return;
            }


            var dialog = ngDialog.open({
                template: 'ChooseMerge',
                data: {
                    'ids': ids,
                    'rows': $scope.igrid.GetSelectedRows(),
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.SelectedRows = $scope.ngDialogData.rows;
                    $scope.SelectedItem = null;
                    $scope.rowselected = function () {
                        if (typeof ($scope.id) != 'undefined' && $scope.id > 0) {
                            var selected = $scope.SelectedRows.find(function (item) {
                                return item.id == $scope.id;
                            });
                            $scope.SelectedItem = selected;
                            $scope.mergename = selected.designid;
                        }
                    }
                    $scope.Selected = function () {
                        $scope.closeThisDialog({
                            'mergename': $scope.mergename,
                            'mergeid': $scope.SelectedRows[0].id,
                            'ids': ids
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        request = {
                            "item": data.value,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "ProductdesignService/Merge",
                            data: srequest
                        }).success(function (result) {
                            $scope.SearchDesign();
                        });
                    }
                }
            });
        }

        $scope.SearchDesign = function () {
            $scope.CurrentDesign = null;

            if (($scope.ProductGallery == undefined || $scope.ProductGallery.ProductID == undefined) &&
                ($scope.ProductGallery == undefined || $scope.ProductGallery.Supplierid == undefined)) {
                $rootScope.Toaster.Info("Please select either Product or Supplier or Both");
                return;
            }

            var srequest = JSON.stringify({
                'Item':
                {
                    'productid': $scope.ProductGallery.ProductID,
                    'supplierid': $scope.ProductGallery.Supplierid,
                }
            });
            $http({
                method: 'POST',
                url: "ProductdesignService/Search",
                data: srequest
            }).success(function (result) {
                $scope.DesignList = result;
                $scope.ShowDesignList();
            });
        }

        $scope.ShowDesignList = function () {
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#grddesignitemlist').empty();
            }
            var igrid = new InfyGrid('grddesignitemlist', '100px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.options.rowHeight = 40;
            igrid.AddSimpleColumn('designid', 'Design', 100);
            igrid.AddReferenceColumn('supplierid', 'Supplier', 200, $scope.SupplierList);
            igrid.AddReferenceColumn('productid', 'Product', 200, $scope.ProductList);

            igrid.AddCustomColumn('sizeid', 'Size', 100, function (item, col) {
                if (item.detail && item.detail.sizes) {
                    var sizelist = [];
                    $.each(item.detail.sizes, function (kk, vv) {
                        sizelist.push($scope.SearchRef(vv, $scope.SizeList));
                    })
                    return sizelist.join(',');
                }
                return '';
            });

            igrid.AddCustomColumn('colourid', 'Colour', 100, function (item, col) {
                if (item.colours) {
                    var sizelist = [];
                    $.each(item.colours, function (kk, vv) {
                        sizelist.push($scope.SearchRef(vv.id, $scope.ColourList));
                    })
                    return sizelist.join(',');
                }
                return '';
            });

            column = igrid.AddCustomColumn('img_', '', 50, function (item, col) {
                if (item.colours == undefined ||
                    item.colours == null) {
                    return '';
                }
                var firstimage = item.colours.find(function (imgitem) {
                    return imgitem.image && imgitem.image != null && imgitem.image != '';
                })
                if (firstimage && firstimage != null) {
                    return '<div style="height: 100%;display: flex;align-items: center;justify-content: center;"><img style="width:100%;height:100%" src="' + col.urlprefix + firstimage.image + '"/></div>';
                }
                return '';
            });

            column.urlprefix = 'File/Show/' + $scope.UserInfo.OrganizationID + '/';

            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-warning btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.Initialize();
            $scope.igrid = igrid;
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Delete')
                    $scope.DeleteDesign(item);
                if (command == 'Edit')
                    $scope.EditDesign(item);
            });
            $scope.igrid.UpdateData($scope.DesignList, '_id');
        }

        $scope.EditDesign = function (item) {
            $timeout(function () {
                $scope.CurrentDesign = item;
                $scope.EditMode = true;
                $scope.$digest();
            }, 10);
        }

        $scope.DeleteDesign = function (item) {
            $.confirm({
                title: 'Delete',
                content: 'Do you want to delete?',
                confirmButton: 'Yes',
                confirmButtonClass: 'btn-info',
                icon: 'fa fa-question-circle',
                animation: 'scale',
                animationClose: 'top',
                opacity: 0.5,
                buttons: {
                    formSubmit: {
                        text: 'Submit',
                        btnClass: 'btn-blue',
                        action: function () {
                            $scope.SaveDesign = function () {
                                var srequest = JSON.stringify({
                                    'Item': item
                                });
                                $http({
                                    method: 'POST',
                                    url: "ProductdesignService/Delete",
                                    data: srequest
                                }).success(function (result) {
                                    $timeout(function () {
                                        $scope.DesignList.splice(array.indexOf(item), 1);
                                        $scope.igrid.UpdateData($scope.DesignList, '_id');
                                        $scope.$digest();
                                    }, 10);
                                    $rootScope.Toaster.Info("Design Deleted");
                                });
                            }

                            return true;
                        }
                    },
                    cancel: function () {
                        //close
                    },
                },

                onContentReady: function () {
                    // bind to events
                    var jc = this;
                    jc.$$formSubmit.trigger('focus');
                    this.$content.find('form').on('submit', function (e) {
                        // if the user submits the form by pressing enter in the field.
                        e.preventDefault();
                        jc.$$formSubmit.trigger('click'); // reference the button and click it
                    });
                }
            });
        }

        $scope.SearchDesignList = function () {
            if ($scope.IsDesignMaster) return;

            $scope.CurrentDesign = null;


            if (($scope.ProductGallery == undefined || $scope.ProductGallery.ProductID == undefined) &&
                ($scope.ProductGallery == undefined || $scope.ProductGallery.Supplierid == undefined)) {
                $rootScope.Toaster.Info("Please select either Product or Supplier or Both");
                return;
            }

            $scope.CurrentColourList = $scope.ColourList;
            $scope.CurrentSizeList = $scope.SizeList;
            var srequest = JSON.stringify({
                'Item':
                {
                    'productid': $scope.ProductGallery.ProductID,
                    'supplierid': $scope.ProductGallery.Supplierid,
                }
            });
            $http({
                method: 'POST',
                url: "ProductdesignService/Search",
                data: srequest
            }).success(function (result) {
                var designlist = [];
                if (result.length > 0) {
                    var sizelist = [];
                    var colourlist = [];
                    $.each(result, function (k, v) {
                        designlist.push(v);
                        if (v.detail && v.detail.sizes) {
                            $.each(v.detail.sizes, function (kk, vv) {
                                if (sizelist.indexOf(vv) < 0)
                                    sizelist.push(vv);
                            })
                        }
                        if (v.colours) {
                            $.each(v.colours, function (kk, vv) {
                                if (colourlist.indexOf(vv.id) < 0)
                                    colourlist.push(vv.id);
                            })
                        }
                    });

                    $scope.CurrentColourList = [];
                    $scope.CurrentSizeList = [];
                    $.each(sizelist, function (k, v) {
                        var exist = $scope.SizeList.find(function (item) { return item.id == v; });
                        if (exist && exist != null)
                            $scope.CurrentSizeList.push(exist);
                    })

                    $.each(colourlist, function (k, v) {
                        var exist = $scope.ColourList.find(function (item) { return item.id == v; });
                        if (exist && exist != null)
                            $scope.CurrentColourList.push(exist);
                    })
                }
                $scope.DesignList = designlist;
            });
        }

        $scope.SaveDesign = function () {
            var srequest = JSON.stringify({
                'Item': $scope.CurrentDesign
            });
            $http({
                method: 'POST',
                url: "ProductdesignService/Save",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Design Updated");
            });
        }

        $scope.ShowImageMap = true;
        $scope.ConfigGrid = function (stocklist, designbased) {
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridResultContainer').empty();
            }
            var igrid = new InfyGrid('gridResultContainer', '100px', '100%', false, true, true, -30);
            igrid.options.showHeaderRow = true;

            if ($scope.ProductGallery.Supplierid == undefined || $scope.ProductGallery.Supplierid == null || $scope.ProductGallery.Supplierid == 0) {
                igrid.AddSimpleColumn('supplier', 'Supplier', 100);
            }

            if (!designbased) {
                igrid.AddSimpleColumn('designid', 'Design', 100);
            }

            igrid.AddSimpleColumn('colour', 'Colour', 100);
            igrid.AddCustomColumn('purchasedate', 'Purchased On', 100, function (item, col) {
                if (item.fpurchase == item.lpurchase) {
                    return $filter('date')(item.fpurchase, 'dd-MMM-yyyy')
                }
                return $filter('date')(item.fpurchase, 'dd-MMM-yyyy') + ' - ' + $filter('date')(item.lpurchase, 'dd-MMM-yyyy')
            });
            igrid.AddCustomColumn('saledate', 'Sold On', 100, function (item, col) {
                if (item.fsales == item.lsales) {
                    return $filter('date')(item.fpurchase, 'dd-MMM-yyyy')
                }
                return $filter('date')(item.fsales, 'dd-MMM-yyyy') + ' - ' + $filter('date')(item.lsales, 'dd-MMM-yyyy')
            });
            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'detail',
                    name: 'Detail',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            $.each($scope.SizeList, function (k, v) {

                igrid.columns = [];
                var column = igrid.AddCustomColumn('purchase_' + v, 'PURC', 50, function (item, col) {
                    var exist = item.stock.find(function (sitem) {
                        return sitem.size == col.size;
                    });
                    if (exist && exist != null) {
                        return exist.bqty + exist.qty;
                    }
                    return 0;
                });
                column.size = v;

                var column = igrid.AddCustomColumn('sales_' + v, 'SOLD', 50, function (item, col) {
                    var exist = item.stock.find(function (sitem) {
                        return sitem.size == col.size;
                    });
                    if (exist && exist != null) {
                        return exist.bqty;
                    }
                    return 0;
                });
                column.size = v;
                var column = igrid.AddCustomColumn('stock_' + v, 'STOCK', 50, function (item, col) {
                    var exist = item.stock.find(function (sitem) {
                        return sitem.size == col.size;
                    });
                    if (exist && exist != null) {
                        return exist.qty;
                    }
                    return 0;
                });
                column.size = v;

                groupColumns.push(
                    {
                        id: v,
                        name: v,
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            });

            igrid.columns = groupColumns;
            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(stocklist, '_id');
        }

        $scope.panelheight = { 'height': '500px' };
        $scope.scrollwidth = { 'width': '800px' };
        $scope.viewpanelheight = { 'height': '500px', 'overflow-y': 'auto' };
        $timeout(function () {
            $scope.LoadAppsettings();
            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 80) + 'px';
                $scope.viewpanelheight.height = '' + (available - 60) + 'px';
            }
        }, 100);

    }]);
