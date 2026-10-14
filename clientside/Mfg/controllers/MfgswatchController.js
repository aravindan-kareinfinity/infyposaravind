var app = angular.module('InfinityLab');
app.controller('MfgswatchController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.SearchCondition = { 'name': '' };

        $scope.AllActionModes = [{ id: 0, name: 'Swatch Registration' }, { id: 1, name: 'Swatch Search' },
        { id: 2, name: 'Folder Creation' }, { id: 3, name: 'Folder Search' },
        { id: 4, name: 'Folder Issue' }, { id: 5, name: 'Folder Received' },
        { id: 6, name: 'Folder Journal' }, { id: 7, name: 'Folder History' },
        { id: 8, name: 'Folder Summary' }, { id: 9, name: 'Samples Manager' }];

        $scope.ActionMode = 1;
        $scope.actionmode = 9;
        $scope.ActionModes = [{ id: 0, name: 'Swatch Registration' }, { id: 1, name: 'Swatch Search' }];
        $scope.SelectOption = function (option) {
            $scope.ActionMode = option;
            if (option == 2) {
                $scope.ActionModes = [{ id: 0, name: 'Swatch Registration' }, { id: 1, name: 'Swatch Search' }]
            }
            if (option == 3) {
                $scope.ActionModes = [{ id: 2, name: 'Folder Creation' }, { id: 3, name: 'Folder Search' },
                { id: 4, name: 'Folder Issue' }, { id: 5, name: 'Folder Received' },
                { id: 6, name: 'Folder Journal' }, { id: 7, name: 'Folder History' },
                { id: 8, name: 'Folder Summary' }]
            }
            if (option == 1) {
                $scope.actionmode = 9;
            }
        }

        $scope.HistoryMode = [{ id: -1, name: 'All' },
        { id: 0, name: 'Issue' }, { id: 1, name: 'Received' },
        { id: 2, name: 'Journal' }];

        $scope.SampleOptions = [{ id: 2, name: 'Issue' },
        { id: 3, name: 'Return' },
        { id: 0, name: 'Report' },
        { id: 1, name: 'Stock Summary' }];

        $scope.SampleModes = [{ id: -1, name: 'All' },
        { id: 0, name: 'Issue' }, { id: 1, name: 'Received' },
            { id: 2, name: 'Pending' }];


        $scope.TailoringMode = [{ id: 3, name: 'No' }, { id: 1, name: 'Work Order' }, { id: 2, name: 'Job Order' }];

        $scope.Entity = {
            name: "",
            "bomsupport": "Yes",
            "mode": "Internal",
            "stagesupport": "Yes",
            stages: [],
            isactive: true,
            version: 1
        };


        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];
        $scope.CurrentStage = null;
        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.WOOrchestration = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.RetailLocation = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.RetailLocation = results;
        });

        $scope.Supplier = restService.Get('ReferencelistService/SUPPLIERLEDGER', true).then(function (results) {
            $scope.Supplier = results;
        });
        $scope.Brands = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brands = results;
        });
        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });
        $scope.Salesmans = restService.Get('ReferencelistService/SALESMEN', true).then(function (results) {
            $scope.Salesmans = results;
        });
        $scope.Employ = restService.Get('ReferencelistService/EMPLOYEELEDGER', true).then(function (results) {
            $scope.Employ = results;
        });

        $scope.ColorList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });
        $scope.ErrorMessage = "";

        $scope.New = function () {
            $scope.WOOrchestration = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }



        $scope.ManageSearchItem = function (source, action, item) {
            if (action == "Delete") {


                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.code + "?",
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
                                request = {
                                    "Item": item,
                                    "SecurityToken": "String content"
                                }
                                var srequest = JSON.stringify(request);

                                if (source == 'Folder') {
                                    $http({
                                        method: 'POST',
                                        url: "Mfg_Swatches_FolderService/Delete",
                                        data: srequest
                                    }).success(function (result) {

                                        if (result) {
                                            $scope.ErrorMessage = "Successfully deleted";
                                            $scope.SwatchList.splice($scope.FolderList.indexOf(item), 1);
                                            $scope.igrid.UpdateData($scope.FolderList);
                                        }

                                    }).error(function (result) {

                                    });
                                }
                                else if (source == 'Swatch') {
                                    $http({
                                        method: 'POST',
                                        url: "Mfg_SwatchesService/Delete",
                                        data: srequest
                                    }).success(function (result) {

                                        if (result) {
                                            $scope.ErrorMessage = "Successfully deleted";
                                            $scope.SwatchList.splice($scope.SwatchList.indexOf(item), 1);
                                            $scope.igrid.UpdateData($scope.SwatchList);
                                        }
                                    }).error(function (result) {

                                    });
                                }
                                else if (source == 'History') {
                                    $http({
                                        method: 'POST',
                                        url: "Mfg_Swatches_HistoryService/Delete",
                                        data: srequest
                                    }).success(function (result) {

                                        if (result) {
                                            $scope.ErrorMessage = "Successfully deleted";
                                            $scope.HistoryList.splice($scope.HistoryList.indexOf(item), 1);
                                            $scope.igrid.UpdateData($scope.HistoryList);
                                        }
                                    }).error(function (result) {

                                    });
                                }


                            }
                        },
                        cancel: function () {
                            //close
                        },
                    },

                    onContentReady: function () {
                        // bind to events
                        var jc = this;
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
                        });
                    }
                });
            }
            else if (action == "Edit") {
                if (source == 'Swatch') {
                    $timeout(function () {
                        $scope.actionmode = 0;
                        $scope.swatch = item;
                        if (new Date(item.invoicedate).getFullYear() < 2000)
                            item.invoicedate = '';
                        if (new Date(item.entrydate).getFullYear() < 2000)
                            item.entrydate = '';
                        if (new Date(item.receivedon).getFullYear() < 2000)
                            item.receivedon = '';
                    }, 20);
                }
                if (source == 'Folder') {
                    $timeout(function () {
                        $scope.actionmode = 2;
                        $scope.folder = item;
                    }, 20);
                }
            }
        }

        $scope.actionchanged = function () {
            $scope.Clear();
            $timeout(function () {
                if ($scope.actionmode == 0) {
                    $scope.setFocusswatchcode();
                } else if ($scope.actionmode == 1) {
                    $scope.setFocusswatchsearchcode();
                } else if ($scope.actionmode == 2) {
                    $scope.setFocusfoldercode();
                } else if ($scope.actionmode == 3) {
                    $scope.setFocusfoldersearchcode();
                } else if ($scope.actionmode == 4) {
                    $scope.setFocusissuereceivedby();
                } else if ($scope.actionmode == 5) {
                    $scope.setFocusreturnedby();
                } else if ($scope.actionmode == 6) {
                    $scope.setFocussalesmenid();
                }
            }, 10);
        }

        $scope.swatch = { 'isactive': true, 'version': 1 };
        $scope.folder = { 'isactive': true, 'version': 1 };
        $scope.igrid = null;
        $scope.RegisterSwatch = function () {
            $scope.swatch.isactive = true;
            request = {
                "Item": $scope.swatch,
                "securitytoken": "String content"
            }
            var ismodified = $scope.swatch && $scope.swatch.id && $scope.swatch.id > 0;
            $http({
                method: 'POST',
                url: "Mfg_SwatchesService/Save",
                data: request
            }).success(function (result) {
                if (result && result.id > 0) {
                    if (!ismodified) $scope.SwatchList.push(result);
                    $scope.ConfigSwatchList();
                    $scope.swatch = { 'isactive': true, 'version': 1 };
                    $rootScope.Toaster.Notice("Swatch Saved successfully!");
                }
            }).error(function (result) {
            });
        }


        $scope.folderchanged = function (source) {
            $scope.CurrentFolder = null;
            var exist = $scope.CurrentFolderStock.find(function (item) {
                return item.id == source.folderid;
            });
            if (typeof (exist) != 'undefined' && exist != null) {
                $scope.CurrentFolder = exist;
                source.swatchesid = exist.swatchesid;
            }
        }
        $scope.CurrentFolder = null;
        $scope.CurrentFolderStock = null;
        $scope.SearchFolderStock = function (design, focus, source, salesmenid) {
            $scope.CurrentFolder = null;
            $scope.CurrentFolderStock = null;
            $scope.folder.isactive = true;
            request = {
                "Item": { 'design': design, 'salesmenid': salesmenid },
                "securitytoken": "String content"
            }
            var ismodified = $scope.folder && $scope.folder.id && $scope.folder.id > 0;
            $http({
                method: 'POST',
                url: "Mfg_Swatches_FolderService/Stock",
                data: request
            }).success(function (result) {
                if (result && result.length > 0) {
                    $scope.CurrentFolderStock = result;
                    if (source && source != null)
                        source.designid = result[0].designid;
                    if (focus == 'issuefolderid') {
                        if (result.length == 1) {
                            $scope.issue.folderid = result[0].id;
                            $scope.folderchanged($scope.issue);
                            $scope.issue.qty = 1;
                            $scope.Add2Issue($scope.issue);
                            return;
                        }
                    }
                    if (focus == 'receivefolderid') {
                        if (result.length == 1) {
                            $scope.receive.folderid = result[0].id;
                            $scope.folderchanged($scope.receive);
                            $scope.receive.qty = 1;
                            $scope.Add2Receive($scope.receive);
                            return;
                        }
                    }

                    $timeout(function () {
                        $scope['setFocus' + focus]();
                    }, 10)
                }
            }).error(function (result) {
            });
        }



        $scope.SaveJournal = function () {
            $scope.journal.isactive = true;
            request = {
                "Items": $scope.FolderInternalList,
                "securitytoken": "String content"
            }
            var ismodified = $scope.journal && $scope.journal.id && $scope.journal.id > 0;
            $http({
                method: 'POST',
                url: "Mfg_Swatches_FolderService/Journal",
                data: request
            }).success(function (result) {
                if (result && result.id > 0) {
                    if (!ismodified) $scope.HistoryList.push(result);
                    $scope.ConfigHistoryList();

                    var salesmenid = $scope.receive.salesmenid;
                    $scope.receive = {
                        'isactive': true, 'version': 1, 'salesmenid': salesmenid
                    };
                    $rootScope.Toaster.Notice("Folder Journal Saved!");
                    $scope.setFocusjournaldesign();
                }
            }).error(function (result) {
            });
        }

        $scope.SaveReceive = function () {
            $scope.receive.isactive = true;
            request = {
                "Items": $scope.FolderInternalList,
                "securitytoken": "String content"
            }
            var ismodified = $scope.receive && $scope.receive.id && $scope.receive.id > 0;
            $http({
                method: 'POST',
                url: "Mfg_Swatches_FolderService/Receive",
                data: request
            }).success(function (result) {
                $scope.HistoryList = result;
                $scope.ConfigHistoryList();
                var receivedby = $scope.receive.receivedby;
                $scope.receive = {
                    'isactive': true, 'version': 1, 'receivedby': receivedby
                };
                $scope.setFocusreturndesign();
                $rootScope.Toaster.Notice("Folder Received successfully!");
            }).error(function (result) {
            });
        }

        $scope.Add2Journal = function (journal) {
            $scope.FolderInternalList.push((JSON.parse(JSON.stringify($scope.journal))));
            $scope.ConfigFolderInternalList();
            journal.design = ''
            $timeout(function () {
                $scope.setFocusjournaldesign();
            }, 10);
        }

        $scope.Add2Receive = function (receive) {

            var qty = 0;
            $.each($scope.FolderInternalList, function (k, v) {
                if (receive.design == v.design) {
                    qty++;
                }
            })

            if (qty >= $scope.CurrentFolder.stock) {
                $rootScope.Toaster.Notice("Salesmen doesn't have folder to receive as per system register!");
                return;
            }

            $scope.FolderInternalList.push((JSON.parse(JSON.stringify($scope.receive))));
            $scope.ConfigFolderInternalList();
            receive.design = ''
            $timeout(function () {
                $scope.setFocusreturndesign();
            }, 10);
        }

        $scope.Add2Issue = function (issue) {


            $scope.FolderInternalList.push((JSON.parse(JSON.stringify($scope.issue))));
            $scope.ConfigFolderInternalList();
            issue.design = ''
            $timeout(function () {
                $scope.setFocusissuedesign();
            }, 10);
        }

        $scope.FolderInternalList = [];
        $scope.ConfigFolderInternalList = function () {

            if ($scope.Foldergrid != null)
                $scope.Foldergrid.destroy();

            var igrid = new InfyGrid('foldergrid', '100px', '100%', true, false, true, 350);
            igrid.AddSimpleColumn('design', 'Design', 150);
            igrid.AddColumn(
                {
                    id: "qty", name: "Qty", field: "qty",
                    width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.Initialize();

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $scope.$apply(function () {
                    if (command == 'Delete') {
                        $scope.FolderInternalList.splice($scope.FolderInternalList.indexOf(item), 1);
                        $scope.Foldergrid.UpdateData($scope.FolderInternalList);
                    }
                });
            });
            $scope.Foldergrid = igrid;
            $scope.Foldergrid.UpdateData($scope.FolderInternalList);
        }

        $scope.SaveIssue = function () {
            $scope.issue.isactive = true;
            request = {
                "Items": $scope.FolderInternalList,
                "securitytoken": "String content"
            }
            var ismodified = $scope.issue && $scope.issue.id && $scope.issue.id > 0;
            $http({
                method: 'POST',
                url: "Mfg_Swatches_FolderService/Issue",
                data: request
            }).success(function (result) {
                $scope.HistoryList = result;
                $scope.ConfigHistoryList();
                var receivedby = $scope.issue.receivedby;
                $scope.issue = {
                    'isactive': true, 'version': 1, 'receivedby': receivedby
                };
                $scope.setFocusissuedesign();
                $rootScope.Toaster.Notice("Folder issued successfully!");
            }).error(function (result) {
            });
        }


        $scope.RegisterFolder = function () {
            $scope.folder.isactive = true;
            request = {
                "Item": $scope.folder,
                "securitytoken": "String content"
            }
            var ismodified = $scope.folder && $scope.folder.id && $scope.folder.id > 0;
            $http({
                method: 'POST',
                url: "Mfg_Swatches_FolderService/Save",
                data: request
            }).success(function (result) {
                if (result && result.id > 0) {
                    if (!ismodified) $scope.FolderList.push(result);
                    $scope.ConfigFolderList();
                    $scope.folder = { 'isactive': true, 'version': 1 };
                    $rootScope.Toaster.Notice("Folder Saved successfully!");
                }
            }).error(function (result) {
            });
        }


        $scope.ConfigFolderList = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.AddSimpleColumn('code', 'Name', 150);
            igrid.AddSimpleColumn('design', 'Design', 100);
            igrid.AddSimpleColumn('product', 'Product', 150);
            igrid.AddSimpleColumn('brand', 'Brand', 100);
            igrid.AddSimpleColumn('noofcolours', 'Colours', 50);
            igrid.AddSimpleColumn('qty', 'Qty', 50).total = true;
            if ($scope.folder.showdetail) {
                igrid.AddSimpleColumn('issue', 'Issue', 50).total = true;
                igrid.AddSimpleColumn('received', 'Received', 50).total = true;
                igrid.AddSimpleColumn('journal', 'Journal', 50).total = true;
                igrid.AddSimpleColumn('stock', 'Stock', 50).total = true;
            }
            igrid.AddReferenceColumn('preparedby', 'Prepared By', 100, $scope.Employ);
            igrid.AddFormatedColumn('creationon', 'Created Date', 80, 'dd-MM-yyyy');

            var commands = "";
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"
            //commands += "<input type='button' class='btn btn-success btn-xs' value='Print'/>"

            igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('Folder', 'Edit', args.item);
                    else if ($(e.target).val() == 'Delete')
                        $scope.ManageSearchItem('Folder', 'Delete', args.item);
                    //else if ($(e.target).val() == 'Print')
                    //    $scope.PrintFolderRow(args.item);
                });
            });

            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.FolderList);
        }

        $scope.Samplegrid = null;
        $scope.SampleList = [];
        $scope.CurrentStock = null;
        $scope.SearchStock = function () {
            request = {
                "Item": {
                    'uid': $scope.sample.uid,
                    'salesmenid': $scope.samples.salesmenid,
                    'locationid': $scope.samples.locationid
                },
                "securitytoken": "String content"
            }
            $http({
                method: 'POST',
                url: "Mfg_SamplesService/Stock",
                data: request
            }).success(function (result) {
                $scope.CurrentStock = result;
                if ($scope.samples.optionid == 3) {
                    if ($scope.CurrentStock.selfstock <= 0) {
                        $rootScope.Toaster.Notice(result.designcode + '(' + $scope.sample.uid + ") doesn't have issues!");
                        return;
                    }
                }
                var exist = $scope.SampleList.find(function (item) {
                    return item.uid == $scope.sample.uid;
                });
                if (exist && exist != null && exist.uid == $scope.sample.uid) {
                    if ($scope.samples.optionid == 3) {
                        var qty = 0;
                        $.each($scope.SampleList, function (k, v) {
                            if (v.uid == $scope.sample.uid)
                                qty++;
                        })
                        if ($scope.CurrentStock.selfstock <= qty) {
                            $rootScope.Toaster.Notice(result.designcode + '(' + $scope.sample.uid + ") doesn't have enogh stock to receive!");
                            return;
                        }
                        $scope.SampleList.push({ 'uid': $scope.sample.uid, 'designcode': result.designcode, 'qty': 1 });
                        $scope.ConfigSampleList();
                    }
                    else {
                        $scope.SampleList.push({ 'uid': $scope.sample.uid, 'designcode': result.designcode, 'qty': 1 });
                        $scope.ConfigSampleList();
                    }
                }
                else {
                    $scope.SampleList.push({ 'uid': $scope.sample.uid, 'designcode': result.designcode, 'qty': 1 });
                    $scope.ConfigSampleList();
                }
            }).error(function (result) {
            });
        }

        $scope.DeleteSampleRow = function (item) {
            var request = {
                "Item": item,
                "securitytoken": "String content"
            }
            $http({
                method: 'POST',
                url: "Mfg_SamplesService/Delete",
                data: request
            }).success(function (result) {
                $scope.SampleReportList.splice($scope.SampleReportList.indexOf(item), 1);
                $scope.igrid.UpdateData($scope.SampleReportList);
            }).error(function (result) {
            });
        }

        $scope.DeleteSampleBarcode = function (item) {
            $scope.SampleList.splice($scope.SampleList.indexOf(item), 1);
            $scope.Samplegrid.UpdateData($scope.SampleList);
        }

        $scope.SaveSample = function () {
            $scope.samples.isactive = true;
            $scope.samples.items = $scope.SampleList;
            request = {
                "Item": $scope.samples,
                "securitytoken": "String content"
            }
            $http({
                method: 'POST',
                url: "Mfg_SamplesService/Save",
                data: request
            }).success(function (result) {
                if (result.sampleitems != null && result.sampleitems.length > 0)
                    $scope.SampleReportList = result.sampleitems;
                else
                    $scope.SampleReportList = [result.sampleitems];
                $scope.ConfigSampleReport($scope.SampleReportList);
                $scope.SampleList = [];
                $scope.Samplegrid.UpdateData($scope.SampleList);
                $rootScope.Toaster.Notice("Sample Saved successfully!");
            }).error(function (result) {
            });
        }


        $scope.DesignStockConfig = function () {
            if (typeof ($scope.StockList.sizenames) == 'undefined' || $scope.StockList.sizenames == null)
                return;

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 25);
            igrid.options.showHeaderRow = true;
            var groupColumns = [];
            igrid.AddSimpleColumn('name', 'Design', 80);
            igrid.AddSimpleColumn('colour', 'Colour', 80);
            igrid.AddSimpleColumn('barcode', 'Barcode', 50);
            $scope.SearchCondition = { ShowDetail: true, ShowFabric: true, ShowPrice: true, ShowSO: true, ShowRemaining: true };
            igrid.AddReferenceColumn('brandid', 'Brand', 100, $scope.Brands);
            igrid.AddReferenceColumn('productid', 'Product', 100, $scope.Product);
            igrid.AddFormatedColumn('forecast', 'Folder', 60, 'N0', 0);
            igrid.AddFormatedColumn('fabricstock', 'Sample', 60, 'N0', 0);

            groupColumns.push(
                {
                    id: 'Filter',
                    name: 'Filter',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            if ($scope.SearchCondition.ShowPrice) {

                igrid.columns = [];
                $.each(['Rate 1', 'Rate 2', 'MRP 1', 'MRP 2'], function (k, v) {
                    var column = igrid.AddCustomColumn('price_' + k, v, 50,
                        function (item, col) {
                            if (item.price != null && item.price[col.Index] > 0)
                                return item.price[col.Index];
                            return '';
                        });
                    column.Index = k;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.price != null && item.price[col.Index] > 0)
                            return item.price[col.Index];
                        return 0;
                    }
                });
                groupColumns.push(
                    {
                        id: 'Price',
                        name: 'Price',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });

            }

            igrid.columns = [];
            $.each($scope.StockList.sizenames, function (k, v) {
                var column = igrid.AddCustomColumn('stock_' + k, v, 40,
                    function (item, col) {
                        if (item.stock[col.Index] > 0)
                            return item.stock[col.Index];
                        return '';
                    });
                column.Index = k;
                column.enablefilter = true;
                column.sortable = true;
                column.cssClass = 'rightalign';
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    if (item.stock[col.Index] > 0)
                        return item.stock[col.Index];
                    return 0;
                }
            });
            groupColumns.push(
                {
                    id: 'Stock',
                    name: 'Stock',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            if ($scope.SearchCondition.ShowSO) {
                igrid.columns = [];
                $.each($scope.StockList.sizenames, function (k, v) {
                    var column = igrid.AddCustomColumn('so_' + k, v, 40,
                        function (item, col) {
                            if (item.so && item.so[col.Index] && item.so[col.Index] > 0)
                                return item.so[col.Index];
                            return '';
                        });
                    column.Index = k;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.so && item.so[col.Index] && item.so[col.Index] > 0)
                            return item.so[col.Index];
                        return 0;
                    }
                });
                groupColumns.push(
                    {
                        id: 'Salesorder',
                        name: 'Sales Order',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            }

            if ($scope.SearchCondition.ShowWIP) {
                igrid.columns = [];
                $.each($scope.StockList.sizenames, function (k, v) {
                    var column = igrid.AddCustomColumn('wip_' + k, v, 40,
                        function (item, col) {
                            if (item.wip && item.wip[col.Index] && item.wip[col.Index] > 0)
                                return item.wip[col.Index];
                            return '';
                        });
                    column.Index = k;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.wip && item.wip[col.Index] && item.wip[col.Index] > 0)
                            return item.wip[col.Index];
                        return 0;
                    }
                });
                groupColumns.push(
                    {
                        id: 'wip',
                        name: 'WIP',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            }
            if ($scope.SearchCondition.ShowRemaining) {
                igrid.columns = [];
                $.each($scope.StockList.sizenames, function (k, v) {
                    var column = igrid.AddCustomColumn('rem_' + k, v, 40,
                        function (item, col) {
                            var stock = 0;
                            var so = 0;
                            if (item.stock && item.stock[col.Index] && item.stock[col.Index] > 0)
                                stock = item.stock[col.Index];
                            if (item.so && item.so[col.Index] && item.so[col.Index] > 0)
                                so = item.so[col.Index];
                            if (stock - so == 0)
                                return '';
                            return stock - so;
                        });
                    column.Index = k;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        var stock = 0;
                        var so = 0;
                        if (item.stock && item.stock[col.Index] && item.stock[col.Index] > 0)
                            stock = item.stock[col.Index];
                        if (item.so && item.so[col.Index] && item.so[col.Index] > 0)
                            so = item.so[col.Index];
                        return stock - so;
                    }
                });

                igrid.AddSimpleColumn('name', 'Design', 100);

                groupColumns.push(
                    {
                        id: 'remaining',
                        name: 'Remaining',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            }
            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.StockList.items);
        }


        $scope.SearchSampleSummary = function () {
            request = {
                "Item": $scope.samples,
                "securitytoken": "String content"
            }
            $http({
                method: 'POST',
                url: "Mfg_SamplesService/Summary",
                data: request
            }).success(function (result) {
                if ($scope.samples.showdetail) {
                    $scope.StockList = result;
                    $scope.DesignStockConfig();
                }
                else {
                    $scope.ConfigSampleSummary(result);
                }
            }).error(function (result) {
            });
        }

        $scope.ConfigSampleSummary = function (result) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '100px', '100%', true, true, true, 0);
            igrid.AddReferenceColumn('productid', 'Product', 100, $scope.Products);
            igrid.AddReferenceColumn('brandid', 'Brand', 100, $scope.Brands);
            igrid.AddSimpleColumn('designcode', 'Design', 120);
            igrid.AddFormatedColumn('folderstock', 'Folder', 70, 'N2', 0);
            igrid.AddFormatedColumn('samplestock', 'Sample', 70, 'N2', 0);
            igrid.AddFormatedColumn('productstock', 'Product', 70, 'N2', 0);
            igrid.AddFormatedColumn('salesorder', 'Salesorder', 70, 'N2', 0);
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result);
        }

        $scope.SearchSample = function () {
            request = {
                "Item": $scope.samples,
                "securitytoken": "String content"
            }
            $http({
                method: 'POST',
                url: "Mfg_SamplesService/Search",
                data: request
            }).success(function (result) {
                $scope.ConfigSampleReport(result);
            }).error(function (result) {
            });
        }

        $scope.ConfigSampleReport = function (result) {

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '100px', '100%', true, true, true, 0);
            igrid.AddSimpleColumn('receiptno', 'Receipt No', 80);
            igrid.AddFormatedColumn('entrydate', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('uid', 'Name', 80);
            igrid.AddSimpleColumn('designcode', 'Design', 120);
            igrid.AddReferenceColumn('salesmenid', 'Salemen', 100, $scope.Employ);
            igrid.AddReferenceColumn('employeeid', 'Employ', 100, $scope.Supplier);

            $scope.SampleModes = [{ id: -1, name: 'All' },
            { id: 0, name: 'Issue' }, { id: 1, name: 'Received' },
                { id: 2, name: 'Pending' }];

            if (typeof ($scope.samples.mode) == 'undefined') $scope.samples.mode = -1;
            if ($scope.samples.mode == 2) {
                var column = igrid.AddCustomColumn('issue', 'Pending', 60,
                    function (item, col) {
                        if (item.qty > 0) return item.qty;
                        return '';
                    });
                column.enablefilter = true;
                column.sortable = true;
                column.cssClass = 'rightalign';
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    if (item.qty > 0) return item.qty;
                    return 0;
                }
            }

            if ($scope.samples.mode == -1 || $scope.samples.mode == 0) {
                var column = igrid.AddCustomColumn('issue', 'Issue', 60,
                    function (item, col) {
                        if (item.qty > 0) return item.qty;
                        return '';
                    });
                column.enablefilter = true;
                column.sortable = true;
                column.cssClass = 'rightalign';
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    if (item.qty > 0) return item.qty;
                    return 0;
                }
            }

            if ($scope.samples.mode == -1 || $scope.samples.mode == 1) {

                column = igrid.AddCustomColumn('return', 'Return', 60,
                    function (item, col) {
                        if (item.qty < 0) return item.qty * -1;
                        return '';
                    });
                column.enablefilter = true;
                column.sortable = true;
                column.cssClass = 'rightalign';
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    if (item.qty < 0) return item.qty * -1;
                    return 0;
                }
            }

            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            //igrid.AddButtonColumn('Print', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Print' class='btn btn-success btn-xs'><i class='fa fa-print' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.EnableTotal();
            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $scope.$apply(function () {
                    if (command == 'Delete') {
                        $scope.DeleteSampleRow(item);
                    }
                    /*if (command == 'Print') {
                        $scope.PrintSampleRow(item);
                    }*/
                });
            });
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result);
            $scope.SampleReportList = result;
        }

        $scope.PrintPDF = function (infygrid) {
            var ids = [];
            $.each(infygrid.GetSelectedRows(), function (k, v) {
                ids.push(v.id);
            })
            if (ids.length == 1) {
                if ($scope.ActionMode == 1) {
                    $rootScope.Download('Mfg_SamplesService/PrintPDF/' +
                        $scope.UserInfo.OrganizationID + '/' +
                        ids[0], 'SampleReceipt.pdf', 'application/pdf');
                } else if ($scope.ActionMode == 3) {
                    $rootScope.Download('Mfg_Swatches_HistoryService/PrintPDF/' +
                        $scope.UserInfo.OrganizationID + '/' +
                        ids[0], 'FolderReceipt.pdf', 'application/pdf');
                }
            }
            else {
                $rootScope.Toaster.Notice("Please select any one of receipt");
            }
        }

        $scope.PrintSampleRow = function (value) {
            $timeout(function () {
                $rootScope.Download('Mfg_SamplesService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + value.id, 'SampleReceipt.pdf', 'application/pdf');
                return;
            }, 10);
        }

        $scope.PrintFolderRow = function (value) {
            $timeout(function () {
                $rootScope.Download('Mfg_Swatches_HistoryService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + value.id, 'FolderReceipt.pdf', 'application/pdf');
                return;
            }, 10);
        }

        $scope.Clear = function () {

            $scope.CurrentStock = null;

            if ($scope.Samplegrid != null)
                $scope.Samplegrid.destroy();

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            if ($scope.Foldergrid != null)
                $scope.Foldergrid.destroy();
        }

        $scope.ConfigSampleList = function () {

            if ($scope.Samplegrid != null)
                $scope.Samplegrid.destroy();

            var igrid = new InfyGrid('samplegrid', '100px', '100%', true, false, true, 350);
            igrid.AddSimpleColumn('uid', 'Name', 100);
            igrid.AddSimpleColumn('qty', 'Qty', 60);
            igrid.AddSimpleColumn('designcode', 'Design', 120);
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.Initialize();

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $scope.$apply(function () {
                    if (command == 'Delete') {
                        $scope.DeleteSampleBarcode(item);
                    }
                });
            });
            $scope.Samplegrid = igrid;
            $scope.Samplegrid.UpdateData($scope.SampleList);
        }

        $scope.HistoryList = [];
        $scope.FolderList = [];
        $scope.SwatchList = [];
        $scope.igrid = null;
        $scope.ConfigSwatchList = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.AddSimpleColumn('code', 'Name', 150);
            igrid.AddSimpleColumn('design', 'Design', 150);
            //igrid.AddSimpleColumn('product', 'Product', 150);
            //igrid.AddSimpleColumn('brand', 'Brand', 150);
            igrid.AddReferenceColumn('supplierid', 'Supplier', 100, $scope.Supplier);
            igrid.AddSimpleColumn('pono', 'PO No', 100);
            igrid.AddSimpleColumn('invoiceno', 'Invoie No', 100);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('entrydate', 'Entry Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('receivedon', 'Received Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('receivedby', 'Received By', 100, $scope.Employ);
            igrid.AddFormatedColumn('requiredmtr', 'Required Meter', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('receivedmtr', 'Received Meter', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('receivedpcs', 'Received Pieces', 80, 'N2', 2).total = true;
            var commands = "";
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('Swatch', 'Edit', args.item);
                    else if ($(e.target).val() == 'Delete')
                        $scope.ManageSearchItem('Swatch', 'Delete', args.item);
                });
            });

            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SwatchList);
        }

        $scope.ConfigHistoryList = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.AddSimpleColumn('receiptno', 'Receipt No', 100);
            igrid.AddSimpleColumn('design', 'Design', 100);
            igrid.AddSimpleColumn('product', 'Product', 100);
            igrid.AddSimpleColumn('brand', 'Brand', 100);
            igrid.AddFormatedColumn('transactionon', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('action', 'Employee', 100);
            igrid.AddSimpleColumn('salesmen', 'Salesmen', 100);
            igrid.AddReferenceColumn('transactiontype', 'Mode', 80, $scope.HistoryMode);
            $.each($scope.HistoryMode, function (k, v) {
                if (k > 0) {
                    var column = igrid.AddCustomColumn('tranaction-' + k, v.name, 60,
                        function (item, col) {
                            if (item.transactiontype == col.transactiontype)
                                return Math.abs(item.qty);
                            return '';
                        });
                    column.transactiontype = v.id;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.transactiontype == col.transactiontype)
                            return Math.abs(item.qty);
                        return 0;
                    }
                }
            })

            if ($scope.historysearch.designid > 0 && $scope.historysearch.referenceid > 0 &&
                $scope.historysearch.transactiontype == -1) {
                var qty = 0;
                $.each($scope.HistoryList, function (k, v) {
                    qty += (v.qty * -1);
                    v.stock = qty;
                });
                igrid.AddFormatedColumn('stock', 'Holdings', 80, 'N0', 0);
            }

            var commands = "";
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('History', 'Edit', args.item);
                    else if ($(e.target).val() == 'Delete')
                        $scope.ManageSearchItem('History', 'Delete', args.item);
                });
            });

            igrid.EnableTotal();
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.HistoryList);
        }


        $scope.foldersummary = {};
        $scope.SearchSummary = function () {
            request = {
                "Item": $scope.foldersummary,
                "securitytoken": "String content"
            }
            $http({
                method: 'POST',
                url: "Mfg_Swatches_FolderService/Summary",
                data: request
            }).success(function (result) {
                $scope.SummaryList = result;
                $scope.ConfigSummaryList();
            }).error(function (result) {
            });
        }


        $scope.ConfigSummaryList = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.AddSimpleColumn('salesmen', 'Salesmen', 100);
            igrid.AddSimpleColumn('design', 'Design', 100);
            igrid.AddSimpleColumn('product', 'Product', 100);
            igrid.AddSimpleColumn('brand', 'Brand', 100);
            igrid.AddSimpleColumn('folder', 'Folder', 100);
            igrid.AddFormatedColumn('folderstock', 'In-Hand', 80, 'N0', 0).total = true;
            igrid.AddFormatedColumn('fgstock', 'Total Product Stock', 80, 'N0', 0).total = true;
            igrid.AddFormatedColumn('sopending', 'Total Pending Order', 80, 'N0', 0).total = true;
            igrid.AddFormatedColumn('soselforder', 'Salesmen Order', 80, 'N0', 0).total = true;
            igrid.AddFormatedColumn('soselfpending', 'Salesmen Pending Order', 80, 'N0', 0).total = true;

            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SummaryList);
        }

        $scope.historysearch = { 'transactiontype': -1 };
        $scope.SearchHistory = function () {
            request = {
                "Item": $scope.historysearch,
                "securitytoken": "String content"
            }
            $http({
                method: 'POST',
                url: "Mfg_Swatches_HistoryService/Search",
                data: request
            }).success(function (result) {
                $scope.HistoryList = result;
                $scope.ConfigHistoryList();
            }).error(function (result) {
            });
        }

        $scope.searchfolder = {};
        $scope.SearchFolderList = function () {
            request = {
                "Item": $scope.folder,
                "securitytoken": "String content"
            }
            $http({
                method: 'POST',
                url: "Mfg_Swatches_FolderService/Search",
                data: request
            }).success(function (result) {
                $scope.FolderList = result;
                $scope.ConfigFolderList();
            }).error(function (result) {
            });
        }

        $scope.swatchsearch = {};
        $scope.SearchSwatchList = function () {
            request = {
                "Item": $scope.swatchsearch,
                "securitytoken": "String content"
            }
            $http({
                method: 'POST',
                url: "Mfg_SwatchesService/Search",
                data: request
            }).success(function (result) {
                $scope.SwatchList = result;
                $scope.ConfigSwatchList();
            }).error(function (result) {
            });
        }


        $scope.SearchSwatch = function (source, focusid, design) {
            request = {
                "Item": {
                    'code': design
                },
                "securitytoken": "String content"
            }
            $http({
                method: 'POST',
                url: "Mfg_SwatchesService/Search",
                data: request
            }).success(function (result) {
                if (result && result.length > 0) {
                    if (source == 'folder') {
                        $scope.folder.swatchesid = result[0].id;
                        $scope.folder.design = result[0].design;
                        $scope.folder.product = result[0].product;
                        $scope.folder.brand = result[0].brand;
                        $scope.folder.designid = result[0].designid;
                    }
                    $scope['setFocus' + focusid]();
                }
                else {
                    $rootScope.Toaster.Notice("Can't find the swatch - " + design);
                }
            }).error(function (result) {
            });
        }

        $scope.GetColourList = function (item) {
            var colourlist = [];
            if (item.sizegroup != null) {
                $.each(item.sizegroup.items, function (k, v) {
                    var exist = colourlist.find(function (fitem) {
                        return fitem.id = v.colourid;
                    })
                    if (typeof (exist) == 'undefined' || exist == null) {
                        colourlist.push({ 'id': v.colourid, 'name': $scope.SearchRef(v.colourid, $scope.ColorList) });
                    }
                })
            }
            return colourlist;
        }

        $scope.SearchDesign = function (source, focusid, design) {
            if (typeof (design) == 'undefined' || design == null || design == '') return;

            request = {
                "Item": {
                    'code': design,
                    'includedetail': true
                },
                "securitytoken": "String content"
            }
            $http({
                method: 'POST',
                url: "MfgdesignService/Search",
                data: request
            }).success(function (result) {
                if (result && result.length > 0) {

                    if (source == 'folder') {
                        $scope.folder.designid = result[0].id;
                        $scope.folder.design = design;
                        $scope.folder.colourlist = $scope.GetColourList(result[0]);
                        $scope.folder.product = $scope.SearchRef(result[0].productid, $scope.Products);
                        $scope.folder.brand = $scope.SearchRef(result[0].brandid, $scope.Brands);

                        if (typeof ($scope.folder.id) == 'undefined' || $scope.folder.id || $scope.folder.id <= 0)
                            $scope.folder.noofcolours = $scope.folder.colourlist.length;
                    }
                    else if (source == 'swatch') {
                        $scope.swatch.designid = result[0].id;
                        $scope.swatch.colourlist = $scope.GetColourList(result[0]);
                    }

                    $scope['setFocus' + focusid]();
                }
                else {
                    $rootScope.Toaster.Notice("Can't find the design - " + design);
                }
            }).error(function (result) {
            });
        }

        $scope.SwatchImageChanged = function (imageid) {
            if (typeof ($scope.swatch.imagelist) == 'undefined' || $scope.swatch.imagelist == null)
                $scope.swatch.imagelist = [];

            if (typeof (imageid) != 'undefined') {
                if (angular.isArray(imageid)) {
                    if (imageid[0] != null && imageid[0] != '') {
                        $scope.swatch.imagelist.splice(0, 0, imageid[0]);
                    }
                }
                else if (imageid != null && imageid != '') {
                    $scope.swatch.imagelist.splice(0, 0, imageid);
                }
            }
            else {
                $scope.swatch.imagelist.splice(0, 0, $scope.billimg);
            }
            $scope.swatchimage = null;
        }

        $scope.RemoveImage = function (item) {
            $scope.swatch.imagelist.splice($scope.swatch.imagelist.indexOf(item), 1);
        }


        $scope.exporttoExcel = function (infygrid, filename, subtitle) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = typeof (subtitle) == 'undefined' ? '' : subtitle;
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);

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

    }]);