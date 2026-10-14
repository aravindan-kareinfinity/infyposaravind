var app = angular.module('InfinityLab');

app.controller('STOInboxScanController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService',
    'ngDialog', '$state', '$timeout', '$filter', 'cntxService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state, $timeout, $filter, cntxService) {
        $scope.Permission = UserDetails.Info;
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;
        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Location = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Location = results;
            results.push({ 'id': 0, 'name': 'All Locations' });
        });


        restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });

        restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
        });

        restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });


        $scope.SelecteOrder = null;

        $scope.ConfigGrid = function (result) {
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridList').empty();
            }
            var igrid = new InfyGrid('gridList', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial(result);

            if ($scope.IsUANBased) {
                igrid.AddSimpleColumn('serialno', 'GLN/UAN', 150);
                igrid.AddSimpleColumn('printingname', 'Item Details', 250);
                igrid.AddSimpleColumn('designid', 'Design', 150);
                igrid.AddSimpleColumn('sizename', 'Size', 100);
            }
            else {
                igrid.AddSimpleColumn('barcode', 'Barcode', 100);

                if ($scope.ShowGLN) {
                    igrid.AddSimpleColumn('serialno', 'GLN/UAN', 100);
                }
                igrid.AddSimpleColumn('printingname', 'Item Details', 140);
            }

            igrid.AddFormatedColumn('mrp', 'MRP', 80, 'N2', 2);

            if ($scope.SupportPriceTypes) {
                igrid.AddSimpleColumn('price', 'Current Price', 100);
                igrid.AddSimpleColumn('uprice', 'Inward Price', 100);
            }

            if ($scope.ScanForOutward) {
                if ($scope.Inwards.showpackingslip) {
                    igrid.AddFormatedColumn('iqty', 'Transfered', 60, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('sqty', 'Packed', 60, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('pqty', 'Scanned', 60, 'N2', 2).total = true;
                } else {
                    igrid.AddFormatedColumn('iqty', 'Transfered', 80, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('sqty', 'Scanned', 80, 'N2', 2).total = true;
                }

                var journalchanged = result.find(function (item) {
                    return item.journalqty && item.journalqty > 0;
                });
                if (journalchanged && journalchanged != null) {
                    igrid.AddFormatedColumn('journalqty', 'Skipped', 80, 'N2', 2).total = true;
                }
            }
            else {
                if ($scope.Inwards.showpackingslip) {
                    igrid.AddFormatedColumn('iqty', 'Transfered', 60, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('sqty', 'Packed', 60, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('pqty', 'Scanned', 60, 'N2', 2).total = true;
                } else {
                    igrid.AddFormatedColumn('iqty', 'Transfered', 80, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('sqty', 'Scanned', 80, 'N2', 2).total = true;
                }

                var journalchanged = result.find(function (item) {
                    return item.journalqty && item.journalqty > 0;
                });
                if (journalchanged && journalchanged != null) {
                    igrid.AddFormatedColumn('journalqty', 'Skipped', 80, 'N2', 2).total = true;
                }
            }


            igrid.EnableTotal();
            igrid.Initialize();
            igrid.setCellCSS('igridbigrows');
            igrid.dataView.getItemMetadata = function (row) {
                var item = igrid.dataView.getItem(row);

                if ($scope.Inwards.showpackingslip) {
                    if (((item.pqty && !isNaN(item.pqty)) ? item.pqty : 0) == item.sqty) {
                        item.style = 'igridbigrowswithgreen';
                        return { cssClasses: 'igridbigrowswithgreen' };
                    }
                    else {
                        item.style = 'igridbigrowswithred';
                        return { cssClasses: 'igridbigrowswithred' };
                    }
                }
                else {
                    if (item.sqty + (item.pqty && !isNaN(item.pqty) ? item.pqty : 0) == item.iqty) {
                        item.style = 'igridbigrowswithgreen';
                        return { cssClasses: 'igridbigrowswithgreen' };
                    }
                    else {
                        item.style = 'igridbigrowswithred';
                        return { cssClasses: 'igridbigrowswithred' };
                    }
                }
            };
            igrid.UpdateData(result, '_id');
            $scope.igrid = igrid;
        }

        $scope.InwardBundleBill = function () {
            var sqty = 0;
            var aqty = 0;
            $.each($scope.Inwards.items, function (k, v) {
                if (v.toid == 0) {
                    v.aqty = v.sqty;
                    sqty += v.sqty;
                    aqty += v.aqty;
                }
            })
            if (sqty != aqty) {
                $rootScope.ShowAlert('Scan Pending', 'Expected :' + aqty + ' Scanned : ' + sqty);
                return;
            }
            request = {
                "item": $scope.Inwards,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BundlesService/InwardUANSTOBill",
                data: srequest
            }).success(function (result) {
                if (result && result.invoiceid && result.invoiceid > 0) {
                    $scope.Inwards.lrentryno = result.lrentryno;
                }
            }).error(function (result) {
            });
        }

        $scope.SavePackingSlips = function () {

            if ($scope.Inwards.showpackingslip && $scope.Inwards.currentpackingcode && $scope.Inwards.currentpackingcode != null) {
                var CurrentPackingSlip = $scope.Inwards.packslip.find(function (item) {
                    return item.code == $scope.Inwards.currentpackingcode;
                })
                if (CurrentPackingSlip == undefined || CurrentPackingSlip == null) {
                    return;
                }

                if ($scope.ScanForOutward) {
                    CurrentPackingSlip.items = [];
                    var inwards = (JSON.parse(JSON.stringify($scope.Inwards)));
                    $.each(inwards.items, function (k, v) {
                        if (v.pqty && v.pqty > 0) {
                            if (v.barcode && v.barcode != null && v.barcode != '') {
                                CurrentPackingSlip.items.push({ 'barcode': v.barcode, 'qty': v.pqty });
                            } else {
                                CurrentPackingSlip.items.push({ 'barcode': v.serialno, 'qty': v.pqty });
                            }
                        }
                    });
                    $scope.Inwards.status = 0;
                }
                else {
                    var inwards = (JSON.parse(JSON.stringify($scope.Inwards)));
                    $.each(inwards.items, function (k, v) {
                        if (v.pqty && v.pqty > 0) {
                            var exitm = CurrentPackingSlip.items.find(function (itm) { return itm.barcode == v.barcode || itm.barcode == v.serialno });
                            if (exitm && exitm != null)
                                exitm.aqty = v.pqty;
                        }
                    });
                }

                $scope.Inwards.packingcode = CurrentPackingSlip.code;
                request = {
                    "item": $scope.Inwards,
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "BundlesService/UpdatePackingSlip",
                    data: srequest
                }).success(function (result) {
                    $scope.Inwards.currentpackingcode = '';
                    $.each($scope.Inwards.items, function (k, v) {
                        v.sqty = 0;
                        v.pqty = 0;
                    });
                    if ($scope.ScanForOutward) {
                        $.each($scope.Inwards.packslip, function (kk, vv) {
                            $.each(vv.items, function (k, v) {
                                var scanneditem = $scope.Inwards.items.find(function (item) {
                                    return item.barcode == v.barcode || item.serialno == v.barcode;
                                });
                                if (scanneditem && scanneditem != null) {
                                    scanneditem.sqty += v.qty;
                                }
                            })
                        });
                    }
                    else {
                        $.each($scope.Inwards.packslip, function (kk, vv) {
                            $.each(vv.items, function (k, v) {
                                var scanneditem = $scope.Inwards.items.find(function (item) {
                                    return item.barcode == v.barcode || item.serialno == v.barcode;
                                });
                                if (scanneditem && scanneditem != null) {
                                    scanneditem.sqty += v.aqty;
                                }
                            })
                        });
                    }
                    $scope.igrid.UpdateData($scope.Inwards.items, '_id');
                }).error(function (result) {
                    $rootScope.Toaster.Info("Saved successfully");
                });
            }
        }

        $scope.ChangeStatusInternal = function () {
            var iqty = 0;
            var sqty = 0;
            $.each($scope.Inwards.items, function (k, v) {
                iqty += v.iqty;
                if ($scope.Inwards.showpackingslip)
                    sqty += v.pqty;
                else
                    sqty += v.sqty;
            });
            if ($scope.Inwards.status == -1) {
                if ($scope.Inwards.eway == undefined ||
                    $scope.Inwards.eway == null) {
                    $scope.Inwards.eway = {};
                }
                $scope.Inwards.eway.issuescannedqty = sqty;
                $scope.Inwards.eway.issueacceptedqty = iqty;
            }
            $scope.Inwards.status = 0;
            request = {
                "item": $scope.Inwards,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BundlesService/UpdateStatus",
                data: srequest
            }).success(function (result) {
                $rootScope.ShowAlert('Success', 'Expected :' + iqty + ' Scanned : ' + sqty);
            }).error(function (result) {
                $rootScope.Toaster.Info("Saved successfully");
            });
        }

        $scope.ChangeStatus = function () {
            var sqty = 0;
            var iqty = 0;
            $.each($scope.Inwards.items, function (k, v) {
                iqty += v.iqty;
                if ($scope.Inwards.showpackingslip)
                    sqty += v.pqty;
                else
                    sqty += v.sqty;
            })

            if (sqty != iqty) {
                $rootScope.ShowAlert('Scan Pending', 'Expected :' + iqty + ' Scanned : ' + sqty);
                $.confirm({
                    title: 'Partial Scanning',
                    content: 'Do you want to remove non-scanned Items? <br> For Billed Invoice items will get debit note',
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
                                $.each($scope.Inwards.items, function (k, v) {
                                    if (v.sqty && v.sqty > 0) {
                                        v.journalqty = v.iqty - v.sqty;
                                        v.iqty = v.sqty;
                                        v.qty = v.sqty;
                                    }
                                    else {
                                        v.journalqty = v.iqty;
                                        v.iqty = 0;
                                        v.qty = 0;
                                    }
                                })
                                $scope.Inwards.bundleitemchanged = true;
                                $scope.ChangeStatusInternal();
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
            else {
                $scope.ChangeStatusInternal();
            }
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'STOCK ORDER - INFORMATION'
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

        $scope.ScannAll = function () {
            $.each($scope.Inwards.items, function (k, v) {
                v.sqty = v.qty;
            })
            $scope.igrid.UpdateData($scope.Inwards.items, '_id');
        }

        $scope.Rescan = function (clearall) {
            if (clearall) {
                $.each($scope.Inwards.items, function (k, v) {
                    v.sqty = 0;
                    v.pqty = 0;
                })
            }
            else {
                $scope.LastScanned.sqty = 0;
                $scope.LastScanned.pqty = 0;
            }
            $scope.igrid.UpdateData($scope.Inwards.items, '_id');
        }

        $scope.LastScanned = null;
        $scope.SearchUAN = function () {
            var existlist = $scope.Inwards.items.filter(function (item) {
                return item.serialno == $scope.UAN;
            });

            var exist = null;
            if (existlist.length == 1) {
                exist = existlist[0];
            } else {
                if ($scope.Inwards.showpackingslip) {
                    exist = existlist.find(function (item) {
                        if (item.pqty == undefined || item.pqty == null)
                            item.pqty = 0;
                        if (item.sqty == undefined || item.sqty == null)
                            item.sqty = 0;
                        return item.iqty >= (item.sqty + 1 + item.pqty);
                    })
                }
                else {
                    exist = existlist.find(function (item) {
                        if (item.sqty == undefined || item.sqty == null)
                            item.sqty = 0;
                        return item.iqty >= (item.sqty + 1);
                    })
                }
            }

            if (exist && exist != null) {
                $scope.LastScanned = exist;
                if ($scope.Inwards.showpackingslip) {
                    if ($scope.LastScanned.pqty == undefined || $scope.LastScanned.pqty == null)
                        $scope.LastScanned.pqty = 0;
                    if ($scope.LastScanned.pqty == $scope.LastScanned.sqty) {
                        $rootScope.ShowAlert('Excess', $scope.UAN + ' Excess Qty ' + (1 + $scope.LastScanned.pqty));
                        return;
                    }
                    $scope.LastScanned.pqty = $scope.LastScanned.pqty + 1;
                }
                else {
                    if ($scope.LastScanned.iqty < $scope.LastScanned.sqty + 1) {
                        $rootScope.ShowAlert('Excess', $scope.UAN + ' Excess Qty ' + ($scope.LastScanned.sqty + 1));
                        return;
                    }
                    $scope.LastScanned.sqty = $scope.LastScanned.sqty + 1;
                }
                $scope.igrid.UpdateData($scope.Inwards.items, '_id');
            }
            else {
                $rootScope.Toaster.Info($scope.UAN + " Not available");
                $scope.LastScanned = null;
            }
            $scope.UAN = '';
        }

        $scope.PrintPackingSlip = function () {
            if ($scope.Inwards.currentpackingcode && $scope.Inwards.currentpackingcode != null) {
                $rootScope.PostDownload("BundlesService/PrintBundle", {
                    'Item': {
                        'id': $scope.Inwards.id,
                        'packingcode': $scope.Inwards.currentpackingcode
                    }
                }, 'bundles.pdf', 'application/pdf');
            }
        }

        $scope.AddPackingSlip = function () {
            if ($scope.Inwards.packslip == undefined ||
                $scope.Inwards.packslip == null)
                $scope.Inwards.packslip = [];

            var packinglist = (JSON.parse(JSON.stringify($scope.Inwards.packslip)));
            var CurrentPackingSlip = {
                'code': $scope.Inwards.code + '(' + ($scope.Inwards.packslip.length + 1) + ')',
                'items': []
            };
            $scope.Inwards.currentpackingcode = CurrentPackingSlip.code;
            packinglist.push(CurrentPackingSlip);
            $scope.Inwards.packslip = packinglist;
            $scope.LoadPackingSlip();
        }

        $scope.LoadPackingSlip = function () {
            if ($scope.Inwards.packslip == undefined || $scope.Inwards.packslip == null)
                $scope.Inwards.packslip = [];
            var CurrentPackingSlip = $scope.Inwards.packslip.find(function (item) {
                return item.code == $scope.Inwards.currentpackingcode;
            })
            if ($scope.ScanForOutward) {
                $.each($scope.Inwards.items, function (k, v) {
                    v.pqty = 0;
                })
                if (CurrentPackingSlip && CurrentPackingSlip != null) {
                    $.each(CurrentPackingSlip.items, function (k, v) {
                        var scanneditem = $scope.Inwards.items.find(function (item) {
                            return item.barcode == v.barcode || item.serialno == v.barcode;
                        });
                        if (scanneditem && scanneditem != null) {
                            scanneditem.pqty = v.qty;
                        }
                    })
                }
            }
            else {
                $.each($scope.Inwards.items, function (k, v) {
                    v.pqty = 0;
                })
                if (CurrentPackingSlip && CurrentPackingSlip != null) {
                    $.each(CurrentPackingSlip.items, function (k, v) {
                        var scanneditem = $scope.Inwards.items.find(function (item) {
                            return item.barcode == v.barcode || item.serialno == v.barcode;
                        });
                        if (scanneditem && scanneditem != null) {
                            scanneditem.pqty = v.aqty;
                            scanneditem.sqty = v.qty;
                        }
                    })
                }
            }
            $scope.igrid.UpdateData($scope.Inwards.items, '_id');
        }

        $scope.LoadDefault = function () {
            if (typeof ($rootScope.UserInfo) == 'undefined' ||
                typeof ($rootScope.UserInfo.appsettings) == 'undefined' ||
                $rootScope.UserInfo.appsettings == null) {
                $timeout(function () {
                    $scope.LoadDefault();
                }, 50);
                return;
            }

            $scope.SupportPriceTypes = $rootScope.UserInfo.appsettings.system_invcol_pricetype || false;
            $scope.system_hidesupplier = UserDetails.Info.p_HIDESUPPLIER.view;
            $scope.system_enableinwardmode = $rootScope.UserInfo.appsettings.system_enableinwardmode || false;
            $scope.system_inwardmode = $rootScope.UserInfo.appsettings.system_inwardmode;
            $scope.IsUANBased = $rootScope.UserInfo.appsettings.mfg_mode == 5;
            $scope.ScanForOutward = false;
            $scope.ShowGLN = $rootScope.UserInfo.appsettings.system_glnbatch ||
                $rootScope.UserInfo.appsettings.system_fixed_barcodes;

            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                request = {
                    "Item": { 'id': $location.search().id },
                    "SecurityToken": "String content"
                }

                if ($location.search().outward &&
                    $location.search().outward == 'true') {
                    $scope.ScanForOutward = true;
                }
                $scope.SearchCondition = {};
                $scope.SearchCondition.validatetolocation = true;
                $scope.SearchCondition.id = $location.search().id;
                request = {
                    "Item": $scope.SearchCondition,
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "BundlesService/Search",
                    data: srequest
                }).success(function (result) {
                    if ($.isArray(result) && result.length > 0) {
                        $scope.Inwards = result[0];
                        $scope.Inwards.showpackingslip = ($scope.Inwards.packslip && $scope.Inwards.packslip != null);
                        if ($scope.Inwards.status == 3) {
                            var pendingqty = 0;
                            $.each($scope.Inwards.items, function (k, v) {
                                if (v.toid == 0) {
                                    pendingqty += v.aqty;
                                    v.sqty = 0;
                                }
                            });
                            $scope.ConfigGrid($scope.Inwards.items);
                        }
                        else {
                            var pendingqty = 0;
                            $.each($scope.Inwards.items, function (k, v) {
                                v.sqty = $scope.Inwards.status >= 0 ? v.iqty : 0;
                                pendingqty += (v.iqty - v.sqty);
                            });
                            $scope.ConfigGrid($scope.Inwards.items);
                            if (pendingqty == 0) {
                                $rootScope.ShowAlert('Status', 'This stock order scanning already done');
                                return;
                            }
                        }

                        if ($scope.Inwards.showpackingslip && $scope.ScanForOutward) {
                            $.each($scope.Inwards.items, function (k, v) {
                                v.sqty = 0;
                            });
                            $.each($scope.Inwards.packslip, function (kk, vv) {

                                $.each(vv.items, function (k, v) {
                                    var scanneditem = $scope.Inwards.items.find(function (item) {
                                        return item.barcode == v.barcode || item.serialno == v.barcode;
                                    });
                                    if (scanneditem && scanneditem != null) {
                                        scanneditem.sqty += v.qty;
                                    }
                                })

                            });
                            $scope.igrid.UpdateData($scope.Inwards.items, '_id');
                        } else {
                            $.each($scope.Inwards.items, function (k, v) {
                                v.sqty = 0;
                            });
                            $.each($scope.Inwards.packslip, function (kk, vv) {

                                $.each(vv.items, function (k, v) {
                                    var scanneditem = $scope.Inwards.items.find(function (item) {
                                        return item.barcode == v.barcode || item.serialno == v.barcode;
                                    });
                                    if (scanneditem && scanneditem != null) {
                                        scanneditem.sqty += v.aqty;
                                    }
                                })

                            });
                            $scope.igrid.UpdateData($scope.Inwards.items, '_id');
                        }
                    }
                });
            }
        }
        $scope.LoadDefault();
    }]);
