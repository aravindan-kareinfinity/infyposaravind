
var app = angular.module('InfinityLab');

app.controller('SaleseditController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', '$filter', 'restService', 'ngDialog', 'localStorageService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, $filter, restService, ngDialog, localStorageService, $timeout) {
        $scope.SearchRef = restService.SearchReference;
        $scope.status = '';
        $scope.ShowSearch = false;
        $scope.Sections = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Sections = results;
        });
        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.Location = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.Product = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Product = results;
        });
        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
        });
        $scope.SalesTax = restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
            $scope.SalesTax = results;
        });
        $scope.PromotionSchemes = restService.Get('ReferencelistService/PROMOTIONS', true).then(function (results) {
            $scope.PromotionSchemes = results;
        });

        $scope.taxdifference = 0;
        $scope.valuedifference = 0;

        $scope.ActionModes = [{ id: '1', name: 'Price %' },
        { id: '2', name: 'Discount %' },
        { id: '3', name: 'Additional Discount %' },
        { id: '4', name: 'Scheme Discount %' },
        { id: '5', name: 'Remove Barcode' }];

        $scope.SearchSources = [{ id: '1', name: 'All Bill' },
        { id: '2', name: 'Credit Bills' },
        { id: '3', name: 'Cash Bills' },
        { id: '4', name: 'B2B Bills' },
        { id: '5', name: 'Card BIlls' }];

        $scope.Entity = {
            "id": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            "stockitems": [],
        };

        $scope.Journal = (JSON.parse(JSON.stringify($scope.Entity)));


        $scope.Clear = function () {
            $scope.status = '';
            $scope.AvailableStock = 0;
            $scope.SyncStock = 0;
            $scope.UploadedStock = 0;
            $scope.SyncMissingStock = 0;
            $scope.StockResult = [];
            $scope.igrid.UpdateData($scope.StockResult);
        }

        $scope.SynchronizeStockComformation = function (fullstock) {
            $.confirm({
                title: 'SYNCHRONIZATION',
                content: 'Do you want to perform the stock synchronization? The changes will be applied immediately!!',
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
                            if (typeof (fullstock) != 'undefined' && fullstock)
                                $scope.SynchronizeFullStock();
                            else
                                $scope.SynchronizeStock();
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

        $scope.SynchronizeStock = function () {


            $scope.status = "Sync Started.";
            var request = {
                'mode': $scope.SearchCondition.mode,
                'companyid': $scope.SearchCondition.companyid,
                'locationid': $scope.SearchCondition.locationid, 'items': []
            };

            var seletedRows = this.igrid.GetSelectedRows();

            $.each(seletedRows, function (k, v) {
                if ((v.salablegoodsid > 0 || v.stockid > 0))
                    request.items.push({ 'stockid': v.stockid, 'salablegoodsid': v.salablegoodsid, 'stock': v.stock });
            });
            var webrequest = {
                "item": request,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(webrequest);
            $http({
                method: 'POST',
                url: "StockService/SyncStock",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Sync Completed.");
                $scope.status = "Sync Completed.";
            }).error(function (result) {
            });
        }

        $scope.GetNumber = function (value) {
            if (typeof (value) == 'number')
                return value;

            if (typeof (value) == 'undefined' || value == '')
                return -9;

            nvalue = parseFloat(value);
            if (isNaN(nvalue))
                return -9;

            return nvalue;
        };


        $scope.SearchCondition = {
            mode: 1,
            locationid: 0,
            productid: 0,
            brandid: 0
        };

        $scope.StockResult = [];

        $scope.SaveBillHistory = function () {

            $scope.totalqty = 0;
            $scope.amount = 0;
            var item = (JSON.parse(JSON.stringify($scope.SearchCondition)));
            item.items = [];
            $.each($scope.ItemList, function (k, v) {
                if (v.edit) {
                    vitem = (JSON.parse(JSON.stringify(v)));
                    vitem.tax = v.ntax.round(4);
                    vitem.salerate = (v.nreceivable / v.nqty).round(4);
                    vitem.saleamount = (v.salerate - (v.ntax / v.nqty)).round(4);
                    vitem.rate = vitem.salerate;
                    vitem.qty = v.nqty;
                    vitem.price = v.nprice;
                    vitem.discount = v.ndiscount;
                    vitem.additionaldiscount = v.nadditionaldiscount;
                    vitem.schemediscount = v.nschemediscount;
                    vitem.schemeid = v.nschemeid;
                    vitem.receivable = v.nreceivable.round(2);
                    vitem.taxpercentage = v.ntaxpercentage;
                    item.items.push(vitem);
                }
            });

            var serviceurl = "BillhistoryService/Bill4Update";
            request = {
                "item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: serviceurl,
                data: srequest
            }).success(function (result) {

                if (result) {
                    $rootScope.Toaster.Info("Updated Successfully.<br>Reloading...");
                    $scope.SearchStock();
                }

            }).error(function (result) {
            });
        }

        $scope.SearchStock = function () {

            $scope.totalqty = 0;
            $scope.amount = 0;

            var serviceurl = "BillhistoryService/SearchBill4Edit";
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: serviceurl,
                data: srequest
            }).success(function (result) {

                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.ItemList = result;
                $scope.igrid.UpdateData(result);

            }).error(function (result) {
            });
        }


        $scope.SearchChanges = function () {


            var serviceurl = "BillhistoryService/Search";
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: serviceurl,
                data: srequest
            }).success(function (result) {


                $scope.SearchResult = result;


                if ($scope.sgrid != null) {
                    $scope.sgrid.grid.destroy();
                    $('#gridSearchContainer').empty();
                }

                var igrid = new InfyGrid('gridSearchContainer', '425px', '100%', true, true, true, 20);
                igrid.options.showHeaderRow = true;

                igrid.AddReferenceColumn('companyid', 'Company', 100, $scope.Company);
                igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.Location);
                igrid.AddFormatedColumn('fromdate', 'From Date', 80, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('todate', 'To Date', 80, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('tax', 'Edited Tax', 80, 'N2', 0).total = true;
                igrid.AddFormatedColumn('receivable', 'Edited Amount', 80, 'N2', 0).total = true;
                igrid.AddSimpleColumn('editedby', 'Edited By', 100);

                var commands = "<input type='button' class='btn btn-primary btn-xs' value='Rollback'/> &nbsp;"
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

                igrid.AddButtonColumn('action', 'ACTION', 175, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Rollback')
                            $scope.RollbackHistory(args.item);
                        else if ($(e.target).val() == 'Delete') {
                            $scope.DeleteHistory(args.item);
                        }
                    });
                });

                igrid.EnableTotal();
                igrid.Initialize();
                $scope.sgrid = igrid;
                $scope.sgrid.UpdateData($scope.SearchResult);

            }).error(function (result) {
            });
        }


        $scope.DeleteHistory = function (item) {
            var serviceurl = "BillhistoryService/Delete";
            request = {
                "Item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: serviceurl,
                data: srequest
            }).success(function (result) {
                if (result) {
                    $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                    $scope.sgrid.UpdateData($scope.SearchResult);
                }
            }).error(function (result) {
            });
        }

        $scope.RollbackHistory = function (item) {
            var serviceurl = "BillhistoryService/RollBack";
            request = {
                "Item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: serviceurl,
                data: srequest
            }).success(function (result) {
                if (result) {
                    $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                    $scope.sgrid.UpdateData($scope.SearchResult);
                }
            }).error(function (result) {
            });
        }

        $scope.exporttoExcel = function (infygrid, filename) {
            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, filename);
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

        $scope.Appsetting_Company = false;
        $scope.LoadApplicationSetting = function () {
            if (typeof ($rootScope.UserInfo) != 'undefined' &&
                typeof ($rootScope.UserInfo.appsettings) != 'undefined') {
                $scope.Appsetting_Company = true;
            }
            else {
                $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50);
            }
        }



        $scope.igrid = null;

        $scope.ConfigGrid = function (result) {
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }


            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 70);
            igrid.options.showHeaderRow = true;
            igrid.AddFormatedColumn('bdate', 'Date', 60, 'dd-MM-yy');
            igrid.AddSimpleColumn('bno', 'No', 100);
            igrid.AddSimpleColumn('product', 'Detail', 100);
            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddFormatedColumn('ptaxpercentage', 'PTax %', 60, 'N0', 0);
            groupColumns = [];
            groupColumns.push(
                {
                    id: 'Details',
                    name: 'Details',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            igrid.columns = [];

            igrid.AddFormatedColumn('qty', 'Qty', 60, 'N2', 2);
            igrid.AddFormatedColumn('price', 'Price', 60, 'N2', 2);
            igrid.AddFormatedColumn('tax', 'Tax', 60, 'N0', 0).total = true;
            var column = igrid.AddCustomColumn('discount', 'Disct', 60, function (item, col) {
                if (item.additionaldiscount + item.schemediscount > 0) return $filter('IndianNumber')(item.additionaldiscount + (item.schemediscount / item.qty), 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.additionaldiscount + item.schemediscount > 0) return item.additionaldiscount + (item.schemediscount / item.qty);
                return 0;
            }
            igrid.AddFormatedColumn('taxpercentage', '%', 60, 'N2', 2);
            igrid.AddFormatedColumn('cost', 'Cost', 60, 'N2', 2);
            var column = igrid.AddCustomColumn('margin', 'Margin', 60, function (item, col) {
                if (item.cost > 0 && item.receivable > 0) {
                    return $filter('IndianNumber')(item.receivable - (item.cost * item.qty), 2);
                }
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.cost > 0 && item.receivable > 0) {
                    return (item.receivable - (item.cost * item.qty)).round(2);
                }
                return 0;
            }
            igrid.AddFormatedColumn('receivable', 'Value', 60, 'N2', 2).total = true;


            groupColumns.push(
                {
                    id: 'Current',
                    name: 'Current',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            igrid.columns = [];
            var column = igrid.AddCustomColumn('edit', '<input type="checkbox" style="z-index: auto;margin-left:5px">', 40, function (item, col) {
                if (item.edit)
                    return '<input type="checkbox" style="z-index: auto;margin-left:10px" checked>';
                else
                    return '<input type="checkbox" style="z-index: auto;margin-left:10px">';
            });
            column.getValue = function (item, col) {
                return item.edit;
            }

            var column = igrid.AddCustomColumn('nqty', 'Qty', 60, function (item, col) {
                if (item.nqty && item.nqty > 0) return $filter('IndianNumber')(item.nqty, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.nqty && item.nqty > 0) return item.nqty;
                return 0;
            }

            var column = igrid.AddCustomColumn('nprice', 'Price', 60, function (item, col) {
                if (item.nprice && item.nprice > 0) return $filter('IndianNumber')(item.nprice, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.nprice && item.nprice > 0) return item.nprice;
                return 0;
            }


            var column = igrid.AddCustomColumn('ntax', 'Tax', 60, function (item, col) {
                if (item.ntax && item.ntax > 0) return $filter('IndianNumber')(item.ntax, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.ntax && item.ntax > 0) return item.ntax;
                return 0;
            }

            var column = igrid.AddCustomColumn('ntaxpercentage', '%', 60, function (item, col) {
                if (item.ntaxpercentage && item.ntaxpercentage > 0) return $filter('IndianNumber')(item.ntaxpercentage, 0);
                return '';
            });
            column.cssClass = 'rightalign';
            column.getValue = function (item, col) {
                if (item.ntaxpercentage && item.ntaxpercentage > 0) return item.ntaxpercentage;
                return 0;
            }

            var column = igrid.AddCustomColumn('ndiscount', 'Disct', 60, function (item, col) {
                if (item.nadditionaldiscount + item.nschemediscount > 0) return $filter('IndianNumber')(item.nadditionaldiscount + (item.nschemediscount / item.nqty), 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.nadditionaldiscount + item.nschemediscount > 0) return item.nadditionaldiscount + (item.nschemediscount / item.nqty);
                return 0;
            }

            var column = igrid.AddCustomColumn('nreceivable', 'Amount', 60, function (item, col) {
                if (item.nreceivable && item.nreceivable > 0) return $filter('IndianNumber')(item.nreceivable, 0);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.nreceivable && item.nreceivable > 0) return item.nreceivable;
                return 0;
            }

            groupColumns.push(
                {
                    id: 'New',
                    name: 'New',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            igrid.columns = [];

            var column = igrid.AddCustomColumn('dqty', 'Qty', 60, function (item, col) {
                if (item.edit && typeof (item.nqty) != 'undefined') return $filter('IndianNumber')(item.qty - item.nqty, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.edit && typeof (item.nqty) != 'undefined') return item.qty - item.nqty;
                return 0;
            }

            var column = igrid.AddCustomColumn('dprice', 'Price', 60, function (item, col) {
                if (item.edit && typeof (item.nprice) != 'undefined') return $filter('IndianNumber')(item.price - item.nprice, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.edit && typeof (item.nprice) != 'undefined') return item.price - item.nprice;
                return 0;
            }


            var column = igrid.AddCustomColumn('dtax', 'Tax', 60, function (item, col) {
                if (item.edit && typeof (item.ntax) != 'undefined') return $filter('IndianNumber')(item.tax - item.ntax, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.edit && typeof (item.ntax) != 'undefined') return item.tax - item.ntax;
                return 0;
            }

            var column = igrid.AddCustomColumn('dreceivable', 'Amount', 60, function (item, col) {
                if (item.edit && typeof (item.nreceivable) != 'undefined') return $filter('IndianNumber')(item.receivable - item.nreceivable, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.edit && typeof (item.nreceivable) != 'undefined') return item.receivable - item.nreceivable;
                return 0;
            }


            groupColumns.push(
                {
                    id: 'Diff',
                    name: 'Different',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            igrid.columns = groupColumns;
            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();
            igrid.grid.onHeaderClick.subscribe(function (e, args) {

                if (args.column.id == 'edit' && $(e.target).is(":checkbox")) {
                    if (igrid.grid.getEditorLock().isActive() && !igrid.grid.getEditorLock().commitCurrentEdit()) {
                        e.preventDefault();
                        e.stopImmediatePropagation();
                        return;
                    }
                    $scope.valuedifference = 0;
                    $scope.taxdifference = 0;
                    if ($(e.target).is(":checked")) {
                        for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                            var item = igrid.grid.getDataItem(i);
                            item.edit = true;
                            item.nprice = item.price;
                            item.ntax = item.tax;
                            item.nqty = item.qty;
                            item.ntaxpercentage = item.taxpercentage;
                            item.nreceivable = item.receivable;
                            item.nadditionaldiscount = item.additionaldiscount;
                            item.ndiscount = item.discount;
                            item.nschemediscount = item.schemediscount;
                            item.nschemeid = item.schemeid;
                            item.deleted = false;
                        }
                    } else {
                        for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                            var item = igrid.grid.getDataItem(i);
                            item.edit = false;
                            item.nprice = 0;
                            item.ntax = 0;
                            item.nqty = 0;
                            item.ntaxpercentage = 0;
                            item.nreceivable = 0;
                            item.nadditionaldiscount = 0;;
                            item.ndiscount = 0;
                            item.nschemediscount = 0;
                            item.nschemeid = 0;
                            item.deleted = false;
                        }
                    }


                    igrid.grid.invalidate();
                    igrid.grid.render();
                    igrid.UpdateAllTotals(igrid.grid);
                    e.stopPropagation();
                    e.stopImmediatePropagation();
                }

            });

            igrid.grid.onClick.subscribe(function (e, args) {
                if (args.grid.getColumns()[args.cell].id == 'edit') {
                    item = args.grid.getDataItem(args.row);

                    if (item.edit &&
                        (item.deleted || (typeof (item.nreceivable) != 'undefined' && item.nreceivable != 0))) {
                        $scope.valuedifference -= (item.receivable - item.nreceivable);
                        $scope.taxdifference -= (item.tax - item.ntax);
                    }

                    item.edit = !item.edit && $(e.target).val() == 'on';

                    if (item.edit) {
                        item.nprice = item.price;
                        item.ntax = item.tax;
                        item.nqty = item.qty;
                        item.ntaxpercentage = item.taxpercentage;
                        item.nreceivable = item.receivable;
                        item.nadditionaldiscount = item.additionaldiscount;
                        item.ndiscount = item.discount;
                        item.nschemediscount = item.schemediscount;
                        item.nschemeid = item.schemeid;
                        item.deleted = false;
                    }
                    else {
                        item.nprice = 0;
                        item.ntax = 0;
                        item.nqty = 0;
                        item.ntaxpercentage = 0;
                        item.nreceivable = 0;
                        item.nadditionaldiscount = 0;;
                        item.ndiscount = 0;
                        item.nschemediscount = 0;
                        item.deleted = false;
                    }

                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                        igrid.UpdateAllTotals(igrid.grid);
                    }, 10);

                }
            });

            //igrid.grid.onCellChange.subscribe(function (e, args) {
            //    var item = args.grid.getDataItem(args.row);
            //    if (args.grid.getColumns()[args.cell].id == 'nprice') {
            //        $scope.compute(item, 'price');
            //        $timeout(function () {
            //            args.grid.invalidateRow(args.row);
            //            args.grid.render();
            //        }, 10);
            //    }
            //    else if (args.grid.getColumns()[args.cell].id == 'ndiscount') {
            //        $scope.compute(item, 'discount');
            //        $timeout(function () {
            //            args.grid.invalidateRow(args.row);
            //            args.grid.render();
            //        }, 10);
            //    }
            //    else if (args.grid.getColumns()[args.cell].id == 'npdiscount') {
            //        $scope.compute(item, 'discountpercent');
            //        $timeout(function () {
            //            args.grid.invalidateRow(args.row);
            //            args.grid.render();
            //        }, 10);
            //    }
            //    else if (args.grid.getColumns()[args.cell].id == 'npdealerprice') {
            //        $scope.compute(item, 'pdealerprice');
            //        $timeout(function () {
            //            args.grid.invalidateRow(args.row);
            //            args.grid.render();
            //        }, 10);
            //    }
            //    else if (args.grid.getColumns()[args.cell].id == 'ndealerprice') {
            //        $scope.compute(item, 'dealerprice');
            //        $timeout(function () {
            //            args.grid.invalidateRow(args.row);
            //            args.grid.render();
            //        }, 10);
            //    }
            //});
            igrid.UpdateData(result);
            $scope.igrid = igrid;
        }

        $scope.findTax = function (amount) {

            if (typeof (items) == 'undefined' && typeof (this.taxpercentage) != 'undefined')
                return { 'cgst': this.taxpercentage / 2, 'sgst': this.taxpercentage / 2 };

            if (this.items == null || this.items.length == 0)
                return null;

            if (this.items.length == 1)
                return this.items[0];

            if (isNaN(amount)) amount = 0;
            for (var i = 0; i < this.items.length; i++) {
                var item = this.items[i];
                if (item.fromamount <= amount && item.toamount >= amount)
                    return item;
            }
            return { 'cgst': 0, 'sgst': 0 };
        }

        $scope.Apply = function () {
            var action = $scope.SearchCondition.actionmode;
            var applyvalue = $scope.SearchCondition.actiontype == 'increase' ? 1 : -1;
            var percent = parseFloat($scope.SearchCondition.actionvalue);
            var minimumbarcode = parseFloat($scope.SearchCondition.actionvalue);
            var minimumqty = parseFloat($scope.SearchCondition.minimumqty);
            minimumbarcode = isNaN(minimumbarcode) ? 0 : minimumbarcode;
            minimumqty = isNaN(minimumqty) ? 0 : minimumqty;

            $scope.valuedifference = 0;
            $scope.taxdifference = 0;
            if (action == "5" && (minimumbarcode > 0 || minimumqty > 0)) {
                var groupbedbillwise = [];
                $.each($scope.ItemList, function (k, v) {
                    if (typeof (v.edit) != 'undefined' && v.edit) {
                        var bill = groupbedbillwise.find(function (item) {
                            return item.bid == v.bid;
                        });
                        if (typeof (bill) == 'undefined') {
                            bill = { 'bid': v.bid, 'items': [] };
                            groupbedbillwise.push(bill);
                        }
                        bill.items.push(v);
                    }
                });
                if (minimumqty > 0) {
                    $.each(groupbedbillwise, function (k, v) {
                        v.items.sort(function (a, b) {
                            return a.qty - b.qty;
                        });
                        for (var i = 0; i < v.items.length; i++) {
                            var vv = v.items[i];
                            if (i < minimumqty) {
                                if (minimumbarcode > 0 && vv.qty > minimumbarcode) {
                                    vv.nqty = minimumbarcode;
                                    var basecost = (vv.nprice - vv.nadditionaldiscount - (vv.nschemediscount / vv.nqty).round(2));
                                    vv.nreceivable = (basecost * vv.nqty).round(2);
                                    vv.ntax = (vv.nreceivable / (100 + vv.ntaxpercentage) * vv.ntaxpercentage).round(4);
                                }
                            } else {
                                vv.nschemediscount = 0;
                                vv.nadditionaldiscount = 0;
                                vv.nprice = 0;
                                vv.ndiscount = 0;
                                vv.nqty = 0;
                                vv.ntax = 0;
                                vv.ntaxpercentage = 0;
                                vv.nreceivable = 0;
                                vv.deleted = true;
                            }

                            if (typeof (vv.nreceivable) != 'undefined' && typeof (vv.ntax) != 'undefined') {
                                $scope.valuedifference += (vv.receivable - vv.nreceivable);
                                $scope.taxdifference += (vv.tax - vv.ntax);
                            }
                        }
                    });
                }
                else {
                    $.each(groupbedbillwise, function (k, v) {
                        v.items.sort(function (a, b) {
                            return a.qty - b.qty;
                        });
                        for (var i = 0; i < v.items.length; i++) {
                            var vv = v.items[i];
                            if (vv.nqty > minimumbarcode) {
                                vv.nqty = minimumbarcode;
                                var basecost = (vv.nprice - vv.nadditionaldiscount - (vv.nschemediscount / vv.nqty).round(2));
                                vv.nreceivable = (basecost * vv.nqty).round(2);
                                vv.ntax = (vv.nreceivable / (100 + vv.ntaxpercentage) * vv.ntaxpercentage).round(4);
                            }
                            if (typeof (vv.nreceivable) != 'undefined' && typeof (vv.ntax) != 'undefined') {
                                $scope.valuedifference += (vv.receivable - vv.nreceivable);
                                $scope.taxdifference += (vv.tax - vv.ntax);
                            }
                        }
                    });
                }
            }
            else {
                $.each($scope.ItemList, function (k, v) {
                    if (typeof (v.edit) != 'undefined' && v.edit) {

                        var taxInfo = $scope.SalesTax.find(function (arritem) {
                            return arritem.id == v.tid;
                        });
                        if (taxInfo) {
                            if (taxInfo.taxsplit == null) {
                                taxInfo.taxsplit = { 'taxpercentage': taxInfo.taxpercentage };
                            }
                            taxInfo.taxsplit.findTax = $scope.findTax;

                            if (action == "1") {
                                v.nprice = (v.price + ((v.price * percent / 100) * applyvalue)).round(2);
                            } else if (action == "2") {
                                v.nprice += v.discount;
                                v.ndiscount = (v.discount + ((v.price * percent / 100) * applyvalue)).round(2);
                                v.nprice -= v.ndiscount;
                            }
                            else if (action == "3") {
                                v.nadditionaldiscount = (v.additionaldiscount + ((v.price * percent / 100) * applyvalue)).round(2);
                            }
                            else if (action == "4") {
                                v.nschemediscount = (v.schemediscount + ((v.price * percent / 100) * applyvalue) / v.qty).round(2);
                                v.nschemeid = $scope.actionschemeid;
                            } else if (action == "5") {
                                v.nschemediscount = 0;
                                v.nadditionaldiscount = 0;
                                v.nprice = 0;
                                v.ndiscount = 0;
                                v.nqty = 0;
                                v.ntax = 0;
                                v.ntaxpercentage = 0;
                                v.nreceivable = 0;
                                v.deleted = true;
                            }

                            if (v.nqty > 0) {
                                var basecost = (v.nprice - v.nadditionaldiscount - (v.nschemediscount / v.nqty).round(2));
                                var taxitem = taxInfo.taxsplit.findTax(basecost);
                                v.ntaxpercentage = taxitem.cgst + taxitem.sgst;
                                v.nreceivable = (basecost * v.nqty).round(2);
                                v.ntax = (v.nreceivable / (100 + v.ntaxpercentage) * v.ntaxpercentage).round(4);
                            }
                        }
                    }
                    if (typeof (v.nreceivable) != 'undefined' && typeof (v.ntax) != 'undefined') {
                        $scope.valuedifference += (v.receivable - v.nreceivable);
                        $scope.taxdifference += (v.tax - v.ntax);
                    }
                });
            }
            $scope.igrid.grid.invalidate();
            $scope.igrid.grid.render();
            $scope.igrid.UpdateAllTotals($scope.igrid.grid);
            $scope.valuedifference = $scope.valuedifference.round(0);
            $scope.taxdifference = $scope.taxdifference.round(0);
        };

        $timeout(function () {
            $scope.ConfigGrid([]);
            $scope.LoadApplicationSetting();
        }, 50);

    }]);

