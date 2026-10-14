
var app = angular.module('InfinityLab');
app.controller('BarcodeController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$rootScope', 'ngDialog', '$timeout', 'localStorageService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $rootScope, ngDialog, $timeout, localStorageService) {
        $scope.InvoiceInfo = { invoiceid: $location.search().invoiceid };
        $scope.totalmeter = 0;
        $scope.totalbarcodes = 0;
        $scope.totalqty = 0;
        $scope.SearchRef = restService.SearchReference;
        $scope.SearchRefDirect = function (id, list) {
            return $scope.SearchRef(id, list);
        }
        $scope.ShowImage = false;
        $scope.message = "";
        $scope.hasselected = false;
        $scope.productItemList = [];
        $scope.EnableList = false;
        $scope.stylecolname = "col-sm-4";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];
        $scope.SupportSalepriceMrpdifference = false;
        $scope.SupportDiscountOnCost = false;
        $scope.SupportMarginOnCost = false;
        $scope.SupportLocationBased = false;
        $scope.SupportBarcodeGenApproval = false;
        $scope.SupportPriceTags = false;
        $scope.SupportPriceTypes = false;
        $scope.SupportMarkDOWN = false;
        $scope.system_invcol_colour = false;
        $scope.system_invcol_size = false;
        $scope.system_invcol_brand = false;
        $scope.system_marginpercentagezero = false;
        $scope.system_barcodeonceprint = false;
        $scope.HideSupplier = false;
        $scope.CanDelete = false;
        $scope.CanEdit = false;
        $scope.CanCreate = false;
        $scope.LoadingRounding = function () {
            if (typeof ($rootScope.UserInfo.appsettings) == 'undefined' || $rootScope.UserInfo.appsettings == null) {
                $timeout($scope.LoadingRounding, 100);
            }
            else {
                $scope.B2BApplication = $rootScope.UserInfo.appsettings.system_isb2bapplication;
                $scope.HideSupplier = UserDetails.Info.p_HIDESUPPLIER.view;
                $scope.SupportPriceTypes = $rootScope.UserInfo.appsettings.system_invcol_pricetype || false;
                $scope.SupportSalepriceMrpdifference = $rootScope.UserInfo.appsettings.system_salepricemrpdifference || false;
                $scope.SupportQtyEdit = $rootScope.UserInfo.appsettings.system_supportreturnpriceqty || false;
                $scope.SupportDiscountOnCost = $rootScope.UserInfo.appsettings.system_discountoncost || false;
                $scope.SupportMarkDOWN = $rootScope.UserInfo.appsettings.system_marginonmarkup || $rootScope.UserInfo.appsettings.system_marginonmarkdown || false;
                $scope.SupportMarginOnCost = $rootScope.UserInfo.appsettings.system_marginoncost || false;
                $scope.CanChangePrice = typeof ($rootScope.UserInfo.p_CANCHANGEPRICEONBARCODE) == 'undefined' ? false : $rootScope.UserInfo.p_CANCHANGEPRICEONBARCODE.view;
                $scope.CanDelete = typeof ($rootScope.UserInfo.p_BARCODEGEN) == 'undefined' ? false : $rootScope.UserInfo.p_BARCODEGEN.delete;
                $scope.CanEdit = typeof ($rootScope.UserInfo.p_BARCODEGEN) == 'undefined' ? false : $rootScope.UserInfo.p_BARCODEGEN.modify;
                $scope.CanCreate = typeof ($rootScope.UserInfo.p_BARCODEGEN) == 'undefined' ? false : $rootScope.UserInfo.p_BARCODEGEN.create;
                $scope.system_barcodeonceprint = $rootScope.UserInfo.appsettings.system_barcodeonceprint || $rootScope.UserInfo.appsettings.system_barcodeonceprint || false;
                $scope.CanResetBarcodePrintCount = typeof ($rootScope.UserInfo.p_BARCODEONCEPRINT) == 'undefined' ? false : $rootScope.UserInfo.p_BARCODEONCEPRINT.view;
                $scope.SupportLocationBased = $rootScope.UserInfo.appsettings.system_locationbasedbarcodes || false;
                $scope.SupportBarcodeGenApproval = $rootScope.UserInfo.appsettings.intray_barcode || false;
                $scope.SupportPriceTags = $rootScope.UserInfo.appsettings.system_invcol_pricetag || false;
                $scope.system_marginpercentagezero = $rootScope.UserInfo.appsettings.system_marginpercentagezero || false;
                $scope.system_invcol_colour = $rootScope.UserInfo.appsettings.system_invcol_colour || false;
                $scope.system_invcol_size = $rootScope.UserInfo.appsettings.system_invcol_size || false;
                $scope.system_invcol_brand = $rootScope.UserInfo.appsettings.system_invcol_brand || false;

                if ($scope.SupportPriceTags) {
                    $scope.PriceTagList = restService.Get('ReferencelistService/PRICETAGS', true).then(function (results) {
                        $scope.PriceTagList = results;
                    });
                }
            }
        }
        $scope.LoadingRounding();

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.PrintVendorInvoice = function () {
            $rootScope.Download('LrinvoiceService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + $scope.Invoice.id, 'vendorinvoice-' + $scope.Invoice.id + '.pdf', 'application/pdf');
            return;
            $timeout(function () {
                var link = document.createElement('a');
                link.download = "vendorinvoice.pdf";
                link.href = 'LrinvoiceService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + $scope.Invoice.id;
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }

        $scope.DefaultSearch = function () {

            if (typeof ($location.search().invoiceid) != 'undefined') {
                $scope.InvoiceInfo = { invoiceid: $location.search().invoiceid };
                $scope.InitializeInvoiceList(true);
            }
            else if (typeof ($location.search().invoiceids) != 'undefined') {
                $scope.SearchCondition.ids = $location.search().invoiceids;
                $scope.Search(true);
            }
            else if (typeof ($location.search().lrid) != 'undefined') {
                $scope.SearchCondition.lrid = $location.search().lrid;
                $scope.Search(true);
            }
            else if (typeof ($scope.InvoiceInfo.invoiceid) == 'undefined') {
                $scope.SwitchPanel();
            }
            else {
                $scope.InvoiceInfo = { invoiceid: $location.search().invoiceid };
                if (typeof ($scope.InvoiceInfo.invoiceid) == 'undefined') {
                    $scope.SwitchPanel();
                }
                else if ($scope.InvoiceInfo.invoiceid.length > 0 || $scope.InvoiceInfo.invoiceid.indexOf(',') > 0) {
                    $scope.InitializeInvoiceList(false);
                }
            }
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'PRODUCT RECEIPT'
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

        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });

        $scope.BrandName = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandName = results;
        });
        $scope.PatternList = restService.Get('ReferencelistService/LIST-PATTERN', true).then(function (results) {
            $scope.PatternList = results;
        });
        $scope.StyleList = restService.Get('ReferencelistService/LIST-STYLE', true).then(function (results) {
            $scope.StyleList = results;
        });
        $scope.SizeList = restService.Get('ReferencelistService/LIST-SIZE', true).then(function (results) {
            $scope.SizeList = results;
        });
        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });
        $scope.SupplierCompany = restService.Get('ReferencelistService/LIST-SUPPLIERCOMPANY', true).then(function (results) {
            $scope.SupplierCompany = results;
        });
        $scope.Suppliers = restService.Get('ReferencelistService/LIST-SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.ColorList = restService.Get('ReferencelistService/LIST-COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });
        $scope.TypeList = restService.Get('ReferencelistService/LIST-TYPE', true).then(function (results) {
            $scope.TypeList = results;
        });

        $scope.MaterialList = restService.Get('ReferencelistService/LIST-MATERIAL', true).then(function (results) {
            $scope.MaterialList = results;
        });

        $scope.SleeveList = restService.Get('ReferencelistService/LIST-SLEEVE', true).then(function (results) {
            $scope.SleeveList = results;
        });

        $scope.FitList = restService.Get('ReferencelistService/LIST-FIT', true).then(function (results) {
            $scope.FitList = results;
        });

        $scope.currentitem = {
            'Details': {}
        };
        $scope.CurrentBarcodeList = [];
        $scope.InvoiceTemplate = {
            "companyid": 0, "invoiceno": null, "invoicedate": "", "invoiceDate": "", "noofpieces": 0, "noofbundle": 0, "baseamount": 0.0,
            "grossamount": 0.0, "totalamount": 0.0, "lrid": 0, "id": 0, "otheramount": 0.0, "modifiedon": "", "createdon": "",
            "createdby": 0, "modifiedby": 0, "version": 1, "suppliercompanyid": 0, "isactive": true, "lrinvoiceitems": null, "lrinvoicetaxitems": null,
            Lrinvoiceitems: []
        }

        $scope.Invoice = (JSON.parse(JSON.stringify($scope.InvoiceTemplate)));

        $scope.InitializeInvoiceList = function (loaddirect) {
            request = {
                "Item": { id: $scope.InvoiceInfo.invoiceid },
                "SecurityToken": "String content"
            }
            if (!loaddirect && typeof ($scope.InvoiceInfo.invoiceid) != 'undefined' && $scope.InvoiceInfo.invoiceid.indexOf(',') > 0) {
                request.Item.ids = $scope.InvoiceInfo.invoiceid;
                request.Item.id = 0;
            }

            $http({
                method: 'POST',
                url: "LrinvoiceService/Search",
                data: request
            }).success(function (result) {
                if (result.Items[0].isvendorbill == true) {
                    $rootScope.Toaster.Info("Barcode Can't be generated for vendor invoice.!");
                    return;
                }
                $scope.ShowSearch = false;
                $scope.Invoice = result.Items[0];
                $scope.Invoice.lrid = result.lrid;
                $scope.Invoice.lrno = result.lrno;
                $scope.Invoice.retaillocationid = result.retaillocationid;
                $scope.Invoice.lrentryno = result.lrentryno;
                $scope.Invoice.status = result.status;
                $scope.Invoice.supplierid = result.supplierid;
                $scope.Invoice.suppliercompanyname = result.suppliercompanyname;
                $scope.Invoice.companyname = result.companyname;
                $scope.Invoice.companyid = result.companyid;
                $scope.Invoice.companytitle = result.companytitle;
                $scope.Invoice.lrnoInfo = result.lrentryno + " / " + result.lrno;
                $scope.Invoice.PendingBarcodes = 0;
                $scope.totalmeter = 0;
                $scope.totalbarcodes = 0;
                $scope.totalqty = 0;
                $scope.totalsalableqty = 0;
                var hasMeterProduct = false;
                $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {

                    var product = $scope.ProductList.find(function (item) {
                        return item.id == v.productid;
                    });

                    if (typeof (product) != 'undefined' && product.sellingmode == 3)
                        hasMeterProduct = true;

                    v.Details = v.Details == null ? {
                        "printingname": "",
                        "brandid": null,
                        "patternid": null,
                        "styleid": null,
                        "sizeid": null,
                        "typeid": null,
                        "colourid": null,
                        "materialid": null,
                        "productid": v.productid,
                        "productname": typeof (product) != 'undefined' && product != null ? product.name : "MISSING",
                    } : v.Details;

                    var cansetsellingprice = true;
                    if ($scope.system_marginpercentagezero && v.marginpercentage < 1) {
                        if ((v.sellingprice - (v.buyingprice + (v.purchasetax / v.buyingqty))) > (v.sellingprice * 3 / 100)) {
                            v.marginpercentage = (($scope.GetNumber(v.sellingprice) / v.buyingprice * 100) - 100).round(2);
                            if ($scope.SupportMarkDOWN)
                                v.marginpercentage = (100 - ((v.buyingprice / v.sellingprice) * 100)).round(2);
                        }
                        else {
                            v.sellingprice = 0;
                            v.mrp = 0;
                            v.finalprice = 0;
                            v.salediscount = 0;
                            v.salediscountpercentage = 0;
                            cansetsellingprice = false;
                            v.Details.pricelist = [];
                        }
                    }

                    if (cansetsellingprice &&
                        $scope.SupportPriceTypes &&
                        $scope.PriceTags.length > 0) {
                        if (typeof (v.Details.pricelist) == "undefined" || v.Details.pricelist == null)
                            v.Details.pricelist = [];
                        $.each($scope.PriceTags, function (k, vv) {
                            var pl = v.Details.pricelist.find(function (item) {
                                return item.id == vv.id;
                            })
                            if (pl == undefined || pl == null)
                                v.Details.pricelist.push({ 'id': vv.id, 'price': cansetsellingprice ? v.sellingprice : 0 });
                        })
                    }

                    v.lrid = $scope.Invoice.lrid;
                    v.validateitem = (v.itemid == 0);
                    if (!v.validateitem)
                        v.lastDetail = (JSON.parse(JSON.stringify(v.Details)));

                    v.newbarcode = (v.actualqty > v.buyingqty ? v.actualqty : v.buyingqty) + v.freeqty - ((v.actualqty < v.buyingqty ? (v.buyingqty - v.actualqty) : 0) + v.damagedqty + v.sellableqty);
                    if (v.actualqty == 0 && v.buyingqty + v.freeqty == v.returnqty)
                        v.newbarcode = 0;

                    v.allowgenerte = v.newbarcode > 0;
                    v.finalprice = v.sellingprice - v.salediscount;
                    $scope.Invoice.PendingBarcodes += v.newbarcode;

                    $scope.totalmeter += typeof (product) != 'undefined' && product != null && product.sellingmode == 3 ? (v.actualqty > v.buyingqty ? v.actualqty : v.buyingqty) : 0;
                    $scope.totalqty += typeof (product) != 'undefined' && product != null && product.sellingmode == 3 ? 1 : ((v.actualqty > v.buyingqty ? v.actualqty : v.buyingqty) + v.freeqty);
                    $scope.totalbarcodes += v.barcodecount;
                    $scope.totalsalableqty += v.sellableqty;
                });
                if (!hasMeterProduct) $scope.totalmeter = 0;

                if ($scope.egrid == undefined || $scope.egrid == null)
                    $scope.EntryGridConfig();
                $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');

            }).error(function (result) {
            });
        };


        //$scope.DisplayInvoice = function (invoice) {
        //    if ($scope.Invoice.Lrinvoiceitems.length > 0) {
        //        $scope.ItemDetails.sno = $scope.Invoice.Lrinvoiceitems[$scope.Invoice.Lrinvoiceitems.length - 1].sno + 1;
        //        if ($scope.Invoice.multipacksno.length > 0) {
        //            $.each($scope.Invoice.multipacksno, function (k, v) {
        //                var list = [];
        //                var qty = 0;

        //                $.each($scope.Invoice.Lrinvoiceitems, function (ik, iv) {
        //                    if (iv.sno == v) {
        //                        list.push(iv);
        //                        qty += iv.buyingqty;
        //                        iv.IsChild = true;
        //                        iv.IsParent = false;
        //                    }
        //                });

        //                var parent = (JSON.parse(JSON.stringify(list[0])));
        //                parent.IsParent = true;
        //                parent.Details.sizeid = -2;
        //                parent.IsChild = false;
        //                parent.buyingqty = qty;
        //                parent.Child = list;
        //                $scope.Invoice.Lrinvoiceitems.push(parent);

        //            });


        //            $scope.Invoice.Lrinvoiceitems.sort(function (first, next) {
        //                return first.sno == next.sno;
        //            });

        //        }
        //    }
        //}


        $scope.Changprintingeqty = function (item) {
            if (item.SelectedItem) {
                //item.newbarcode = item.buyingqty - item.sellableqty;
                item.newbarcode = (item.actualqty > item.buyingqty ? item.actualqty : item.buyingqty) + item.freeqty - ((item.actualqty < item.buyingqty ? (item.buyingqty - item.actualqty) : 0) + item.damagedqty + item.sellableqty);
                if (item.actualqty == 0 && item.buyingqty == item.returnqty)
                    item.newbarcode = 0;

                item.finalprice = item.sellingprice - item.salediscount;
                if (item.newbarcode > 0) {
                    var proditem = $scope.ProductList.find(function (itm) {
                        return itm.id == item.productid;
                    })
                    if (proditem != null && proditem.sellingmode != 1) {
                        item.newbarcode = 1;
                        item.maxnewbarcode = 1;
                    }
                    else {
                        item.maxnewbarcode = item.newbarcode;
                    }
                }
                $scope.currentitem = {
                    'Details': {}
                };
                $scope.CurrentBarcodeList = [];
                $scope.hasselected = true;
            }
            else {
                var fistselected = $scope.Invoice.Lrinvoiceitems.find(function (item) {
                    return item.SelectedItem;
                });


                $scope.hasselected = typeof (fistselected) != 'undefined' && fistselected != null;
            }
        }



        $scope.MapProduct = function (item) {

            $scope.SelectAll = false;
            $scope.SelectAllChanged();

            if (item.barcodes == "NOT GENERATED" && item.sellableqty > 0)
                item.sellableqty = 0;

            //item.newbarcode = item.buyingqty - (item.damagedqty + item.sellableqty);
            item.newbarcode = (item.actualqty > item.buyingqty ? item.actualqty : item.buyingqty) + item.freeqty - ((item.actualqty < item.buyingqty ? (item.buyingqty - item.actualqty) : 0) + item.damagedqty + item.sellableqty);
            if (item.actualqty == 0 && item.buyingqty == item.returnqty)
                item.newbarcode = 0;

            if (item.newbarcode > 0) {
                var proditem = $scope.ProductList.find(function (itm) {
                    return itm.id == item.productid;
                })
                if (proditem != null && proditem.sellingmode != 1) {
                    item.newbarcode = 1;
                    item.maxnewbarcode = 1;
                }
                else {
                    item.maxnewbarcode = item.newbarcode;
                }
            }
            if (item.barcodes == null || item.barcodes == 'null') {
                item.barcodes = 'NOT GENERATED';
            }
            else if ($scope.EnableList) {
                var BarcodeList = [];
                BarcodeList.push({
                    'id': item.id, 'productid': item.productid, 'qty': item.sellableqty,
                    'code': item.barcodes, 'name': item.Details.productname, 'sno': item.sno,
                    'uan': item.uan
                });
                var request = {
                    "Items": BarcodeList,
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "LrinvoiceService/SearchBarcode",
                    data: srequest
                }).success(function (result) {
                    $scope.CurrentBarcodeList = result;
                }).error(function (result) {
                });
            }
            item.allowgenerte = item.newbarcode > 0;
            $scope.currentitem = item;
            $scope.message = '';
            $scope.hideimage = false;
        };

        $scope.CaptureImage = function () {
            $scope.ShowImageDialog(function (value) {
                if (value && value != null)
                    $scope.currentitem.imgid = value;
            })
        }

        $scope.ShowImageDialog = function (callback) {
            var dialog = ngDialog.open({
                template: 'UpdateImage',
                data: [$scope],
                width: '660px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    $scope.Billitemid = 0;
                    $scope.ShowSnap = false;
                    $scope.Capture = function (closewindow) {
                        var canvas = document.getElementById('canvas');
                        var context = canvas.getContext('2d');
                        var video = document.getElementById('video');
                        context.drawImage(video, 0, 0, 640, 480);
                        $scope.ShowSnap = true;
                        if (closewindow) {
                            $scope.Save();
                        }
                    }

                    $scope.Save = function () {
                        var canvas = document.getElementById('canvas');
                        var imgData = canvas.toDataURL();
                        var request = {
                            "Item": {
                                'image': imgData,
                                'type': 'IMAGE',
                                'lrinvoiceitemid': $scope.ngDialogData[0].currentitem.id
                            },
                            "SecurityToken": "String content"
                        }

                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "StockService/StoreImage",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(result);
                        });
                    }

                    $timeout(function () {

                        var video = document.getElementById('video');

                        // Get access to the camera!
                        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
                            // Not adding `{ audio: true }` since we only want video now
                            navigator.mediaDevices.getUserMedia({ video: true }).then(function (stream) {
                                //video.src = window.URL.createObjectURL(stream);
                                video.srcObject = stream;
                                video.play();
                            });
                        } else if (navigator.getUserMedia) { // Standard
                            navigator.getUserMedia({ video: true }, function (stream) {
                                video.src = stream;
                                video.play();
                            }, errBack);
                        } else if (navigator.webkitGetUserMedia) { // WebKit-prefixed
                            navigator.webkitGetUserMedia({ video: true }, function (stream) {
                                video.src = window.webkitURL.createObjectURL(stream);
                                video.play();
                            }, errBack);
                        } else if (navigator.mozGetUserMedia) { // Mozilla-prefixed
                            navigator.mozGetUserMedia({ video: true }, function (stream) {
                                video.srcObject = stream;
                                video.play();
                            }, errBack);
                        }

                        //if (typeof ($scope.ngDialogData[0].result.billitems) != 'undefined' &&
                        //    $scope.ngDialogData[0].result.billitems.length > 0) {

                        //    var billitem = $scope.ngDialogData[0].result.billitems.find(function (item) {
                        //        return item.qty < 0;
                        //    });

                        //    if (typeof (billitem) != 'undefined' && billitem != null) {
                        //        $scope.Billitemid = billitem.id;
                        //    }
                        //}
                        $scope.setFocuscapture();
                    }, 100);


                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape' && data.value != '') {
                        callback(data.value);
                    }
                    else {
                        callback();
                    }
                }
            });
        }

        $scope.DeleteBarcode = function () {
            var stocklist = [];
            $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                if (typeof (v.SelectedItem) != 'undefined' && v.SelectedItem) {
                    stocklist.push(v.id);
                }
            });

            if (stocklist == 0)
                return;

            var request = {
                "Item": { 'Items': stocklist },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceitemsService/DeleteBarcode",
                data: srequest
            }).success(function (result) {

                if (result) {
                    $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                        if (typeof (v.SelectedItem) != 'undefined' && v.SelectedItem) {
                            v.sellableqty = 0;
                            v.newbarcode = v.buyingqty + v.freeqty - ((v.actualqty > 0 ? (v.buyingqty - v.actualqty) : 0) + v.damagedqty + v.sellableqty);
                            if (v.actualqty == 0 && v.buyingqty == v.returnqty)
                                v.newbarcode = 0;

                            if (v.newbarcode > 0) {
                                var proditem = $scope.ProductList.find(function (itm) {
                                    return itm.id == v.productid;
                                })
                                if (proditem != null && proditem.sellingmode != 1) {
                                    v.newbarcode = 1;
                                    v.maxnewbarcode = 1;
                                }
                                else {
                                    v.maxnewbarcode = v.newbarcode;
                                }
                            }

                            v.allowgenerte = v.newbarcode > 0;
                            v.barcode = '';
                            v.barcodes = "NOT GENERATED";
                            $scope.Invoice.PendingBarcodes += v.newbarcode;
                        }
                    });
                    $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
                }

            }).error(function (result) {
            });
        }

        $scope.SearchProductRef = function (invoiceitem) {
            try {
                var prditem = $scope.ProductList.find(function (item) {
                    return invoiceitem.productid == item.id;
                });
                if ((prditem.producttype == 6 || prditem.producttype == 7)
                    && invoiceitem.Details.sellingname != null && invoiceitem.Details.sellingname != '') {
                    return invoiceitem.Details.sellingname;
                }
                return prditem.name;
            } catch (ex) {
                return 'Exception';
            }
        }

        $scope.UpdatePrice = function () {
            var stocklist = [];
            $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                if (typeof (v.SelectedItem) != 'undefined' && v.SelectedItem) {
                    stocklist.push(v);
                }
            });

            if (stocklist == 0)
                return;

            var request = {
                "Items": stocklist,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceitemsService/UpdatePrice",
                data: srequest
            }).success(function (result) {
                if (result.Success) {
                    $rootScope.Toaster.Info("Saved Successfully.!");
                    $.each(result.Items, function (k, v) {
                        var itm = $scope.Invoice.Lrinvoiceitems.find(function (itm) {
                            return itm.id == v.id;
                        });
                        itm.version = v.version;
                        itm.sellingprice = v.sellingprice;
                        itm.mrp = v.mrp;
                        itm.salediscount = v.salediscount;
                        itm.finalprice = itm.sellingprice - itm.salediscount;
                    });
                    $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });
        };

        $scope.GetItem = function (items, id) {
            var result = items.find(function (item) {
                return item.id == id;
            });
            if (typeof (result) == 'undefined')
                return { 'id': 0, 'name': '' };
            return result;
        }

        $scope.GetProductName = function (item) {

            var product = $scope.GetItem($scope.ProductList, item.productid);
            if (product.id == 0) return;
            if (product.producttype == 0) {
                return product.name;
            }
            if (product.producttype == 6) {
                var nameparts = [];
                var itm = $scope.GetItem($scope.StyleList, item.Details.styleid);
                if (itm.id > 0) nameparts.push(itm.name);
                var itm = $scope.GetItem($scope.PatternList, item.Details.patternid);
                if (itm.id > 0) nameparts.push(itm.name);
                var itm = $scope.GetItem($scope.BrandName, item.Details.brandid);
                if (itm.id > 0) nameparts.push(itm.name);
                var itm = $scope.GetItem($scope.ColorList, item.Details.colourid);
                if (itm.id > 0) nameparts.push(itm.name);
                return nameparts.join('-');
            }
            if (product.producttype == 7) {
                var nameparts = [];
                var itm = $scope.GetItem($scope.StyleList, item.Details.styleid);
                if (itm.id > 0) nameparts.push(itm.name);
                var itm = $scope.GetItem($scope.BrandName, item.Details.brandid);
                if (itm.id > 0) nameparts.push(itm.name);
                nameparts.push(product.code);
                return nameparts.join('-');
            }
        };

        $scope.UpdateItemDetail = function (item, update) {

            if (typeof (update) == 'undefined')
                update = false;

            if (typeof (item.Details) == 'undefined')
                item.Details = {};

            if (!update && !item.validateitem) {
                item.validateitem = typeof (item.lastDetails) == 'undefined'
                    || item.lastDetails == null || (item.Details.brandid != item.lastDetails.brandid
                        || item.Details.patternid != item.lastDetails.patternid
                        || item.Details.styleid != item.lastDetails.styleid || item.Details.sizeid != item.lastDetails.sizeid || item.Details.typeid != item.lastDetails.typeid
                        || item.Details.colourid != item.lastDetails.colourid || item.Details.materialid != item.lastDetails.materialid);
            }

            if (!update && item.Details.productid != item.productid)
                item.validateitem = true;

            if (update || item.validateitem) {
                item.Details.productid = item.productid;
                item.Details.productname = $scope.SearchRef(item.productid, $scope.ProductList, '');
                item.Details.brandname = $scope.SearchRef(item.Details.brandid, $scope.BrandName, '');
                item.Details.typename = $scope.SearchRef(item.Details.typeid, $scope.TypeList, '');
                item.Details.patternname = $scope.SearchRef(item.Details.patternid, $scope.PatternList, '');
                item.Details.materialname = $scope.SearchRef(item.Details.materialid, $scope.MaterialList, '');
                item.Details.colourname = $scope.SearchRef(item.Details.colourid, $scope.ColorList, '');
                item.Details.sizename = $scope.SearchRef(item.Details.sizeid, $scope.SizeList, '');
                item.Details.stylename = $scope.SearchRef(item.Details.styleid, $scope.StyleList, '');
                item.Details.printingname = item.Details.productname;
                item.Details.sellingname = $scope.GetProductName(item);
            }
        }

        //$scope.SaveAll = function () {
        //    var stocklist = [];

        //    $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
        //        if (typeof (v.SelectedItem) != 'undefined' && v.SelectedItem) {

        //            v.Details.printingname = $scope.SearchRef(v.productid, $scope.ProductList, '');

        //            stocklist.push(v);
        //        }
        //    });

        //    var request = {
        //        "Items": stocklist,
        //        "securitytoken": "String content"
        //    }

        //    var srequest = JSON.stringify(request);
        //    $http({
        //        method: 'POST',
        //        url: "LrinvoiceitemsService/SaveMultipleItems",
        //        data: srequest
        //    }).success(function (result) {
        //        if (result.Success) {
        //            $rootScope.Toaster.Info("Saved Successfully.!");
        //        }
        //        else {
        //            $rootScope.Toaster.Error(result.ErrorMessage);
        //        }

        //    }).error(function (result) {
        //    });
        //}

        $scope.SaveMultiple = function (item) {

            if (item.Details.brandid > 0 ||
                item.Details.colourid > 0 ||
                item.Details.materialid > 0 ||
                item.Details.patternid > 0 ||
                item.productid > 0 ||
                item.Details.styleid > 0 ||
                item.Details.typeid > 0 ||
                item.Details.sizeid > 0) {

                var stocklist = [];
                item.itemids = stocklist;
                $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                    if (typeof (v.SelectedItem) != 'undefined' && v.SelectedItem) {
                        stocklist.push(v.id);
                    }
                });

                if (item.itemids.length == 0)
                    return;

                $scope.UpdateItemDetail(item, true);

                var request = {
                    "Item": item,
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "LrinvoiceitemsService/SaveMultiple",
                    data: srequest
                }).success(function (result) {
                    if (result.Success) {

                        $.each(result.Items, function (k, v) {
                            var itm = $scope.Invoice.Lrinvoiceitems.find(function (itm) {
                                return itm.id == v.id;
                            });
                            itm.itemid = v.itemid;
                            itm.version = v.version;
                            itm.Details.sizeid = v.Details.sizeid;
                            itm.Details.productid = v.Details.productid;
                            itm.Details.sellingname = v.Details.sellingname;
                            itm.productid = v.productid;
                        });

                        $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
                        $rootScope.Toaster.Info("Saved Successfully.!");
                    }
                    else {
                        $rootScope.Toaster.Error(result.ErrorMessage);
                    }

                }).error(function (result) {
                });
            }
            else {
                $rootScope.Toaster.Info("Please change any one of attribute..");
            }
        }

        $scope.Save = function (item) {

            if (typeof (item.id) == 'undefined' || item.id == 0) {
                $scope.SaveMultiple(item);
                return;
            }
            $scope.UpdateItemDetail(item);

            request = {
                "Item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrinvoiceitemsService/Save",
                data: srequest
            }).success(function (result) {
                if (result.Success) {

                    if (typeof ($scope.currentitem.barcode) != 'undefined' && $scope.currentitem.barcode.length > 0) {
                        $scope.InitializeInvoiceList(true);
                    }

                    $scope.currentitem = result.Item;
                    result.Item.validateitem = (result.Item.itemid == 0);
                    if (result.Item.validateitem)
                        result.Item.lastDetail = (JSON.parse(JSON.stringify(result.Item.Details)));
                    if (result.ErrorMessage == 'hasCreatedNewItem') {
                        item.itemid = result.Item.itemid;
                        $rootScope.Toaster.Info("Item newly created");
                    }
                    else {
                        $rootScope.Toaster.Info("Saved Successfully.!");
                    }
                    $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });

        };



        $scope.SelectAllChanged = function () {
            $scope.currentitem = {
                'Details': {}
            };
            $scope.CurrentBarcodeList = [];
            $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                v.SelectedItem = $scope.SelectAll;
                $scope.Changprintingeqty(v);
            });
        };

        $scope.GenarateBarcodeMultipleInternal = function () {
            if (!$scope.SupportLocationBased) {
                $scope.GenarateBarcodeMultiple(0);
            }
            else {
                if ($scope.Invoice.retaillocationid > 0) {
                    $scope.GenarateBarcodeMultiple($scope.Invoice.retaillocationid);
                }
                else {

                    var stocklist = [];
                    $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                        if (typeof (v.SelectedItem) != 'undefined' && v.SelectedItem) {
                            stocklist.push(v);
                        }
                    });

                    $scope.ShowLocationBasedBarcode(stocklist, function (data) {
                        $scope.GenarateBarcodeMultiple(data);
                    });
                }
            }
        }

        $scope.GenarateBarcodeMultiple = function (retaillocationid) {

            if ($scope.Invoice.allowgenerte == false) {
                if ($scope.SupportBarcodeGenApproval == true)
                    $rootScope.Toaster.Error("Barcode generation requires approval.");
                else if ($scope.Invoice.buyerinvoiceid > 0)
                    $rootScope.Toaster.Error("Barcode generation not avalable for this invoice.");
                else
                    $rootScope.Toaster.Error("Please complete the status of invoice.");
                return;
            }

            var stocklist = [];
            request = {
                "Items": stocklist,
                "securitytoken": "String content"
            }

            $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                if (typeof (v.SelectedItem) != 'undefined' && v.SelectedItem) {
                    $scope.UpdateItemDetail(v);
                    stocklist.push(v);
                    v.companyid = $scope.Invoice.companyid;
                    v.lrinvoiceid = $scope.Invoice.id;
                    v.printingname = v.Details.printingname;
                    //v.retaillocationid = retaillocationid;
                }
            });

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "StockService/GenarateBarcodeMultiple",
                data: srequest
            }).success(function (result) {
                if (result.Success) {

                    $.each(result.Items, function (k, v) {
                        var item = $scope.Invoice.Lrinvoiceitems.find(function (itm) {
                            return itm.id == v.id;
                        });
                        if (typeof (item) != 'undefined' && item != null) {
                            item.itemid = v.itemid;
                            item.barcodes = v.barcodes;
                            item.barcode = '';
                            var blist = eval(v.barcodes);
                            $.each(blist, function (kk, vv) {
                                item.barcode += (item.barcode.length > 0 ? ',' : '') + vv;
                            });
                            item.version = v.version;
                            item.sellableqty = v.sellableqty;
                            item.newbarcode = 0;
                            item.SelectedItem = false;
                            $scope.Invoice.PendingBarcodes -= item.sellableqty;
                        }
                    });
                    $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
                    $rootScope.Toaster.Info("GenarateBarcode  Successfully.!");
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }
            }).error(function (result) {
            });
        };

        $scope.ShowDNCN = function () {

            var barcodelist = [];
            var allbarcodegenerated = true;
            $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                if (typeof (v.SelectedItem) != 'undefined' && v.SelectedItem) {
                    barcodelist.push(JSON.parse(JSON.stringify(v)));
                }
            });

            if (!allbarcodegenerated) {
                $rootScope.Toaster.Info("Barcode generated items only can generate credit or debit note");
                return;
            }

            var dialog = ngDialog.open({
                template: 'DNCNDialog',
                data: {
                    'BarcodeList': barcodelist,
                    'Parent': $scope,
                    'restService': restService
                },
                width: '650px',
                controller: ['$scope', '$timeout', '$rootScope', function ($scope, $timeout, $rootScope) {
                    $scope.BarcodeList = $scope.ngDialogData.BarcodeList
                    $scope.Parent = $scope.ngDialogData.Parent;
                    $scope.Locations = $scope.ngDialogData.restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
                        $scope.Locations = results;
                    });

                    $scope.pricechanged = function (item) {
                        if (item.nbuyingprice && item.nbuyingprice != null &&
                            item.nbuyingprice != '' && $scope.GetNumber(item.nbuyingprice) > 0) {
                            item.nactualqty = '';
                        }
                        $scope.compute();
                    }

                    $scope.qtychanged = function (item) {
                        if (item.nactualqty && item.nactualqty != null &&
                            item.nactualqty != '' && $scope.GetNumber(item.nactualqty) > 0) {
                            item.nbuyingprice = '';
                        }
                        $scope.compute();
                    }

                    $scope.compute = function (item) {
                        $scope.qtyadded = 0;
                        $scope.qtyremoved = 0;
                        $scope.valinc = 0;
                        $scope.valdec = 0;
                        $scope.taxinc = 0;
                        $scope.taxdec = 0;
                        $.each($scope.BarcodeList, function (k, v) {

                            v.isvalid = false;
                            if (v.nbuyingprice && v.nbuyingprice != null && $scope.GetNumber(v.nbuyingprice) > 0) {
                                var nbuyingprice = $scope.GetNumber(v.nbuyingprice);
                                if (nbuyingprice > v.buyingprice) {
                                    $scope.valinc += (nbuyingprice - v.buyingprice) * v.buyingqty;
                                    $scope.taxinc += ((((nbuyingprice - v.buyingprice) * v.buyingqty) * v.purchasetaxpercentage) / 100).round(2);
                                    v.isvalid = true;
                                } else if (nbuyingprice < v.buyingprice) {
                                    $scope.valdec += (v.buyingprice - nbuyingprice) * v.buyingqty;
                                    $scope.taxdec += ((((v.buyingprice - nbuyingprice) * v.buyingqty) * v.purchasetaxpercentage) / 100).round(2);
                                    v.isvalid = true;
                                }
                            }

                            if (v.nactualqty && v.nactualqty != null && $scope.GetNumber(v.nactualqty) > 0) {
                                var nactualqty = $scope.GetNumber(v.nactualqty);
                                if (nactualqty > v.buyingqty) {
                                    $scope.qtyadded += (nactualqty - v.buyingqty);
                                    $scope.valinc += (nactualqty - v.buyingqty) * v.buyingprice;
                                    $scope.taxinc += ((((nactualqty - v.buyingqty) * v.buyingprice) * v.purchasetaxpercentage) / 100).round(2);
                                    v.isvalid = true;
                                } else if (nactualqty < v.buyingqty) {
                                    $scope.qtyremoved += (v.buyingqty - nactualqty);
                                    $scope.valdec += (v.buyingqty - nactualqty) * v.buyingprice;
                                    $scope.taxdec += ((((v.buyingqty - nactualqty) * v.buyingprice) * v.purchasetaxpercentage) / 100).round(2);
                                    v.isvalid = true;
                                }
                            }

                        })

                        $scope.crnotevalue = 0;
                        $scope.drnotevalue = 0;
                        if ($scope.valinc < $scope.valdec) {
                            $scope.drnotevalue = $scope.valdec - $scope.valinc;
                            $scope.drnotevalue += $scope.taxdec - $scope.taxinc;
                        } else if ($scope.valinc > $scope.valdec) {
                            $scope.crnotevalue = $scope.valinc - $scope.valdec;
                            $scope.crnotevalue += $scope.taxinc - $scope.taxdec;
                        }
                    }

                    $scope.qtyadded = 0;
                    $scope.qtyremoved = 0;
                    $scope.valinc = 0;
                    $scope.valdec = 0;
                    $scope.taxinc = 0;
                    $scope.taxdec = 0;
                    $scope.drnotevalue = 0;
                    $scope.crnotevalue = 0;
                    $scope.Selected = function () {

                        if ($scope.qtyadded > 0 && $scope.qtyremoved > 0) {
                            $rootScope.Toaster.Info("Credit/Debit Can't be merged, please do it as 2 different entries!");
                            return;
                        }

                        if ($scope.valinc > 0 && $scope.valdec > 0) {
                            $rootScope.Toaster.Info("Credit/Debit Can't be merged, please do it as 2 different entries!");
                            return;
                        }

                        var requestitems = [];
                        $.each($scope.BarcodeList, function (k, v) {
                            if (v.isvalid)
                                requestitems.push(v);
                        });
                        $scope.closeThisDialog(
                            {
                                'item':
                                {
                                    'id': $scope.Parent.Invoice.id,
                                    'lrinvoicereturnitems': requestitems,
                                    'qtyadded': $scope.qtyadded,
                                    'qtyremoved': $scope.qtyremoved,
                                    'valinc': $scope.valinc,
                                    'valdec': $scope.valdec,
                                    'taxinc': $scope.taxinc,
                                    'taxdec': $scope.taxdec,
                                    'drnotevalue': $scope.drnotevalue,
                                    'crnotevalue': $scope.crnotevalue
                                }
                            });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        var request = data.value;
                        var srequest = JSON.stringify(data.value);

                        $http({
                            method: 'POST',
                            url: "LrinvoicereturnService/CreateDB_CRNotes",
                            data: srequest
                        }).success(function (result) {
                            if (result.id < 0) {
                                $rootScope.Toaster.Info(result.errormessage);
                                $scope.ShowDNCNError(request, result.errormessage);
                            }
                            else {
                                $rootScope.Toaster.Info("Credit/Debit Note Generated!");
                            }
                        });
                    }
                }
            });
        }

        $scope.ShowDNCNError = function (request, errormessage) {
            var dialog = ngDialog.open({
                template: 'DNCNOverrideDialog',
                data: {
                    'Parent': $scope,
                    'request': request,
                    'errormessage': errormessage
                },
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Reasons = [{ 'id': 'New', 'name': 'Create' }, { 'id': 'Override', 'name': 'Override' }, { 'id': 'Extend', 'name': 'Extend' }];
                    $scope.Parent = $scope.ngDialogData.Parent;
                    $scope.request = $scope.ngDialogData.request;
                    $scope.errormessage = $scope.ngDialogData.errormessage;
                    $scope.Selected = function () {
                        $scope.closeThisDialog($scope.request);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        var request = data.value;
                        var srequest = JSON.stringify(data.value);
                        $http({
                            method: 'POST',
                            url: "LrinvoicereturnService/CreateDB_CRNotes",
                            data: srequest
                        }).success(function (result) {
                            if (result.id < 0) {
                                $rootScope.Toaster.Info(result.errormessage);
                            }
                            else {
                                $rootScope.Toaster.Info("Credit/Debit Note Generated!");
                            }
                        });
                    }
                }
            });
        }

        $scope.ShowLocationBasedBarcode = function (barcodelist, callback) {

            if (typeof (barcodelist) == 'undefined') {
                barcodelist = [];
                $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                    if (typeof (v.SelectedItem) != 'undefined' && v.SelectedItem) {
                        v.tstocklocationid = v.stocklocationid;
                        barcodelist.push(v);
                    }
                });
            }
            var dialog = ngDialog.open({
                template: 'ChooseLocation',
                data: {
                    'BarcodeList': barcodelist,
                    'callback': callback,
                    'Parent': $scope,
                    'restService': restService
                },
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.BarcodeList = $scope.ngDialogData.BarcodeList
                    $scope.Parent = $scope.ngDialogData.Parent;
                    $scope.Locations = $scope.ngDialogData.restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
                        $scope.Locations = results;
                    });
                    $scope.retaillocationid = 0;
                    $scope.Selected = function () {
                        $scope.closeThisDialog({ 'BarcodeList': $scope.BarcodeList, 'callback': $scope.ngDialogData.callback });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        var requestitems = [];
                        $.each(data.value.BarcodeList, function (k, v) {
                            requestitems.push({
                                'id': v.id, 'stocklocationid': v.tstocklocationid, 'sellableqty': v.sellableqty
                            });
                        });

                        var request = { 'Items': requestitems };
                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "LrinvoiceitemsService/ChangeStockLocation",
                            data: srequest
                        }).success(function (result) {
                            if (result) {
                                $rootScope.Toaster.Info("Location Changed Successfully.!");
                                $.each(data.value.BarcodeList, function (k, v) {
                                    v.stocklocationid = v.tstocklocationid;
                                });
                            } else {
                                $rootScope.Toaster.Info("Location Failed to Change!");
                            }
                        });
                    }
                }
            });
        }

        $scope.GenerateBarcodeInternal = function (item) {
            if (!$scope.SupportLocationBased) {
                $scope.GenerateBarcode(item);
            }
            else {
                if ($scope.Invoice.retaillocationid > 0) {
                    item.retaillocationid = $scope.Invoice.retaillocationid;
                    $scope.GenerateBarcode(item);
                }
                else {
                    $scope.ShowLocationBasedBarcode([item], function (data) {
                        item.retaillocationid = data;
                        $scope.GenerateBarcode(item);
                    });
                }
            }
        }

        $scope.GenerateBarcode = function (item) {

            request = {
                "Item": item,
                "securitytoken": "String content"
            }

            item.companyid = $scope.Invoice.companyid;
            item.lrinvoiceid = $scope.Invoice.id;
            item.printingname = item.Details.printingname;
            item.version = item.version;

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "StockService/GenarateBarcode",
                data: srequest
            }).success(function (result) {
                if (result.Success) {
                    item.barcodes = result.Item.barcodes;
                    item.barcode = '';
                    var blist = eval(result.Item.barcodes);
                    $.each(blist, function (k, v) {
                        item.barcode += (item.barcode.length > 0 ? ',' : '') + v;
                    });
                    item.version = result.Item.version;
                    $scope.Invoice.PendingBarcodes += item.sellableqty;
                    item.sellableqty = result.Item.sellableqty;
                    $scope.Invoice.PendingBarcodes -= item.sellableqty;

                    item.newbarcode = (item.actualqty > item.buyingqty ? item.actualqty : item.buyingqty) + item.freeqty - ((item.actualqty < item.buyingqty ? (item.buyingqty - item.actualqty) : 0) + item.damagedqty + item.sellableqty);
                    if (item.actualqty == 0 && item.buyingqty == item.returnqty)
                        item.newbarcode = 0;
                    item.allowgenerte = item.newbarcode > 0;

                    $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
                    $rootScope.Toaster.Info("GenarateBarcode Successfully.!");
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }
            }).error(function (result) {
            });
        };


        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete invoice ' + item.invoiceno + "?",
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
                                $scope.ItemDetails.productid = $scope.ProductSearch.ProductID;
                                $scope.Invoice.Items.push($scope.ItemDetails);
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
            else if (action == "Edit") {
                $scope.InvoiceInfo = {};
                $scope.InvoiceInfo.invoiceid = item.id;
                $scope.InitializeInvoiceList(true);
            }
        }

        $scope.PrintSelected = function (isdirect, lrinvoiceitemid) {

            var requestitem = {
                'reportid': 0, 'companyid': $scope.Invoice.companyid,
                'companytitle': $scope.Invoice.companytitle, 'lrid': $scope.Invoice.lrid,
                'lrno': $scope.Invoice.lrno, 'lrentryno': $scope.Invoice.lrentryno,
                'invoiceid': $scope.Invoice.id, 'items': []
            };
            var BarcodeList = [];
            if (isdirect) {
                if (typeof (lrinvoiceitemid) == 'undefined' || lrinvoiceitemid == 0) {
                    $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                        if (typeof (v.SelectedItem) != 'undefined' && v.SelectedItem) {
                            requestitem.items.push(v.id);
                        }
                    });
                }
                else {
                    requestitem.items.push(lrinvoiceitemid);
                }

                if (requestitem.items.length == 0) {
                    $rootScope.ShowAlert('Print', 'Please select item to Print');
                    return;
                }
            }
            else {
                $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                    if (typeof (v.SelectedItem) != 'undefined' && v.SelectedItem) {
                        requestitem.items.push(v.id);
                        if (v.Details.productname == null)
                            v.Details.productname = $scope.SearchRef(v.productid, $scope.ProductList)
                        BarcodeList.push({
                            'id': v.id, 'productid': v.productid, 'qty': v.sellableqty - v.returnqty, 'code': v.barcodes, 'name': v.Details.productname, 'sno': v.sno,
                            'uan': v.uan
                        });
                    }
                });

                if (requestitem.items.length == 0) {
                    $rootScope.ShowAlert('Print', 'Please select items to Print');
                    return;
                }
            }

            if (isdirect) {
                $scope.ChoseReport(requestitem);
            }
            else {
                request = {
                    "Items": BarcodeList,
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "LrinvoiceService/SearchBarcode",
                    data: srequest
                }).success(function (result) {
                    $scope.ShowBarcodeList(requestitem, result);
                }).error(function (result) {
                });
            }
        }

        $scope.CollectImage = function () {

            var BarcodeList = [];
            if ($scope.currentitem.Details.productname == null)
                $scope.currentitem.Details.productname = $scope.SearchRef($scope.currentitem.productid, $scope.ProductList)

            BarcodeList.push({
                'id': $scope.currentitem.id, 'productid': $scope.currentitem.productid, 'qty': $scope.currentitem.sellableqty, 'code':
                    $scope.currentitem.barcodes, 'name': $scope.currentitem.Details.productname, 'sno': $scope.currentitem.sno,
                'uan': $scope.currentitem.uan
            });

            var request = {
                "Items": BarcodeList,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/SearchBarcode",
                data: srequest
            }).success(function (result) {

                var dialog = ngDialog.open({
                    template: 'UploadImage',
                    data: {
                        'BarcodeList': result,
                        'ParentScope': $scope
                    },
                    width: '500px',
                    controller: ['$scope', '$timeout', '$parse', '$http', function ($scope, $timeout, $parse, $http) {
                        $scope.BarcodeList = $scope.ngDialogData.BarcodeList;
                        $scope.ParentScope = $scope.ngDialogData.ParentScope;

                        $scope.ImageDeleted = function (item) {
                            var request = {
                                "Item": { 'barcode': item.code, 'imageid': '', 'action': 1 },
                                "securitytoken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "StockService/Image",
                                data: srequest
                            }).success(function (result) {
                                item.imageid = '';
                            }).error(function (result) {
                            });
                        }

                        $scope.ImageUploaded = function (uploadList, scope) {
                            var item = $parse('item')(scope);
                            var request = {
                                "Item": { 'barcode': item.code, 'imageid': uploadList[0], 'action': 2 },
                                "securitytoken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "StockService/Image",
                                data: srequest
                            }).success(function (result) {

                            }).error(function (result) {
                            });
                        }
                    }],
                    className: 'ngdialog-theme-plain'
                });
                dialog.closePromise.then(function (data) {
                    {

                    }
                });

            }).error(function (result) {
            });
        }

        $scope.PriceChanged = function (item) {
            item.manualprice = true;
            var value = $scope.GetNumber(item.sellingprice);
            if (!$scope.SupportSalepriceMrpdifference)
                item.mrp = value;

            if (typeof (item.buyingprice) == 'undefined' || item.buyingprice == null || isNaN(item.buyingprice))
                item.buyingprice = item.basebuyingprice;

            var basevalue = $scope.SupportDiscountOnCost ? item.basebuyingprice : item.buyingprice;
            var basesalevalue = $scope.SupportMarginOnCost ? basevalue : item.sellingprice;

            item.marginpercentage = ((item.sellingprice / basevalue * 100).round(2) - 100).round(2);
            item.salediscount = Math.round(basesalevalue * item.salediscountpercentage / 100, 0);
            $scope.salediscountChanged(item);
        }

        $scope.MRPChanged = function (item) {
            item.manualprice = true;
            var value = $scope.GetNumber(item.mrp);
            if (!$scope.SupportSalepriceMrpdifference)
                item.sellingprice = value;
            if (typeof (item.buyingprice) == 'undefined' || item.buyingprice == null || isNaN(item.buyingprice))
                item.buyingprice = item.basebuyingprice;

            var basevalue = $scope.SupportDiscountOnCost ? item.basebuyingprice : item.buyingprice;
            var basesalevalue = $scope.SupportMarginOnCost ? basevalue : item.sellingprice;

            item.marginpercentage = ((item.sellingprice / basevalue * 100).round(2) - 100).round(2);
            item.salediscount = Math.round(basesalevalue * item.salediscountpercentage / 100, 0);
            $scope.salediscountChanged(item);
        }

        $scope.GetNumber = function (value) {
            if (typeof (value) == 'number')
                return value;

            if (typeof (value) == 'undefined' || value == '')
                return 0

            nvalue = parseFloat(value);
            if (isNaN(nvalue))
                return 0;

            return nvalue;
        };

        $scope.salediscountPercentageChanged = function (item) {
            var basePrice = $scope.SupportDiscountOnCost ? item.basebuyingprice : item.sellingprice;

            item.salediscount = Math.round(basePrice * $scope.GetNumber(item.salediscountpercentage) / 100, 0);
            item.finalprice = item.sellingprice - item.salediscount;

            item.salestax = (item.buyingqty * (basePrice - item.salediscount) / (item.salestaxpercentage + 100) * item.salestaxpercentage).round(2);
        };

        $scope.salediscountChanged = function (item) {
            var basePrice = $scope.SupportDiscountOnCost ? item.basebuyingprice : item.sellingprice;
            item.salediscountpercentage = Math.round(item.salediscount / basePrice * 100, 0);
            item.finalprice = item.sellingprice - item.salediscount;
            item.salestax = (item.buyingqty * (basePrice - item.salediscount) / (item.salestaxpercentage + 100) * item.salestaxpercentage).round(2);
        };

        $scope.finalpriceChanged = function (item) {
            var basePrice = $scope.SupportDiscountOnCost ? item.basebuyingprice : item.sellingprice;
            var basesalevalue = $scope.SupportMarginOnCost ? basePrice : item.sellingprice;

            item.salediscount = item.sellingprice - item.finalprice;
            item.salediscountpercentage = (item.salediscount / basesalevalue * 100).round(2);
            item.salediscount = item.sellingprice - item.finalprice;
            item.salestax = (item.buyingqty * (item.finalprice) / (item.salestaxpercentage + 100) * item.salestaxpercentage).round(2);
        };


        $scope.ResetPrint = function (v) {

            var requestitem = v;
            var BarcodeList = [];
            BarcodeList.push({
                'id': v.id, 'productid': v.productid, 'qty': v.sellableqty - v.returnqty, 'code': v.barcodes, 'name': v.Details.productname, 'sno': v.sno,
                'uan': v.uan
            });
            request = {
                "Items": BarcodeList,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/SearchBarcode",
                data: srequest
            }).success(function (result) {

                var dialog = ngDialog.open({
                    template: 'ResetBarecodes',
                    data: {
                        'requestitem': requestitem,
                        'BarcodeList': result
                    },
                    width: '750px',
                    controller: ['$scope', '$timeout', function ($scope, $timeout) {
                        $scope.BarcodeList = $scope.ngDialogData.BarcodeList
                        $scope.requestitem = $scope.ngDialogData.requestitem;
                        $scope.TotalCopy = 0;
                        $scope.selectchanged = function (allsection) {
                            $scope.TotalCopy = 0;
                            $.each($scope.BarcodeList, function (k, v) {
                                if (allsection != undefined) {
                                    v.selected = allsection;
                                }
                                if (v.selected) {
                                    var cpy = parseInt(v.count);
                                    if (!isNaN(cpy))
                                        $scope.TotalCopy += cpy;
                                }
                            });
                        }

                        $scope.SelectAllChanged = function (items) {
                            $.each(items, function (k, v) {
                                v.selected = $scope.selectall;
                            });
                            $scope.selectchanged();
                        };

                        $.each($scope.ngDialogData.BarcodeList, function (k, v) {
                            v.selected = true;
                            if (v.sellingmode == 3)
                                v.count = 2;
                            else if (v.sellingmode == 2)
                                v.count = v.qty;
                            else
                                v.count = 1;
                        });


                        $scope.SetSelection = function () {
                            var range = $scope.fixno.split('-');
                            if (range.length != 2) return;
                            var from = parseInt(range[0]);
                            var to = parseInt(range[1]);
                            if (to < from)
                                to = from + to;

                            $.each($scope.ngDialogData.BarcodeList, function (k, v) {
                                v.selected = v.no >= from && v.no <= to;
                            });
                            $scope.selectchanged();
                        }

                        $scope.Selected = function () {
                            var blist = [];
                            $.each($scope.ngDialogData.BarcodeList, function (k, v) {
                                if (v.selected)
                                    blist.push({ 'code': v.code,'qty':v.count});
                            });
                            $scope.closeThisDialog(blist);
                        };
                        $scope.selectchanged();
                    }],
                    className: 'ngdialog-theme-plain'
                });
                dialog.closePromise.then(function (data) {
                    {
                        if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                            request = {
                                "Items": data.value,
                                "securitytoken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "StockService/ResetBarcodePrintCount",
                                data: srequest
                            }).success(function (result) {

                            });

                        }
                    }
                });

            }).error(function (result) {
            });


           
        }

        $scope.ShowBarcodeList = function (requestitem, BarcodeList) {
            var dialog = ngDialog.open({
                template: 'ChooseBarecodes',
                data: {
                    'requestitem': requestitem,
                    'BarcodeList': BarcodeList
                },
                width: '750px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.BarcodeList = $scope.ngDialogData.BarcodeList
                    $scope.requestitem = $scope.ngDialogData.requestitem;
                    $scope.TotalCopy = 0;
                    $scope.selectchanged = function (allsection) {
                        $scope.TotalCopy = 0;
                        $.each($scope.BarcodeList, function (k, v) {
                            if (allsection != undefined) {
                                v.selected = allsection;
                            }
                            if (v.selected) {
                                var cpy = parseInt(v.copy);
                                if (!isNaN(cpy))
                                    $scope.TotalCopy += cpy;
                            }
                        });
                    }

                    $scope.SelectAllChanged = function (items) {
                        $.each(items, function (k, v) {
                            v.selected = $scope.selectall;
                        });
                        $scope.selectchanged();
                    };

                    $.each($scope.ngDialogData.BarcodeList, function (k, v) {
                        v.selected = true;
                        if (v.sellingmode == 3)
                            v.copy = 2;
                        else if (v.sellingmode == 2)
                            v.copy = v.qty;
                        else
                            v.copy = 1;
                    });

                    $scope.SetCopy = function () {

                        var noofcopy = 0;
                        if (typeof ($scope.fixcopy) != 'undefined' && $scope.fixcopy.length > 0) {
                            var command = $scope.fixcopy.substring(0, 1);
                            noofcopy = parseFloat($scope.fixcopy.replace('+', '').replace('*', '').replace('/', ''));
                            if (noofcopy > 0) {
                                $.each($scope.ngDialogData.BarcodeList, function (k, v) {
                                    if (v.selected == true) {
                                        if (command == '*')
                                            v.copy = v.copy * noofcopy;
                                        else if (command == '+')
                                            v.copy = v.copy + noofcopy;
                                        else if (command == '/')
                                            v.copy = Math.floor(v.qty / noofcopy);
                                        else
                                            v.copy = noofcopy;
                                    }
                                });
                                $scope.selectchanged();
                            }
                        }
                    };

                    $scope.SetSelection = function () {
                        var range = $scope.fixno.split('-');
                        if (range.length != 2) return;
                        var from = parseInt(range[0]);
                        var to = parseInt(range[1]);
                        if (to < from)
                            to = from + to;

                        $.each($scope.ngDialogData.BarcodeList, function (k, v) {
                            v.selected = v.no >= from && v.no <= to;
                        });
                        $scope.selectchanged();
                    }

                    $scope.Selected = function () {
                        var blist = [];
                        $.each($scope.ngDialogData.BarcodeList, function (k, v) {
                            if (v.selected)
                                blist.push(v);
                        });
                        $scope.requestitem.filterbarcode = blist;
                        $scope.requestitem.packagesize = $scope.packagesize;
                        $scope.closeThisDialog($scope.requestitem);
                    };
                    $scope.selectchanged();
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.ChoseReport(data.value);
                    }
                }
            });
        }

        $scope.Mismatch = function (item) {
            $rootScope.Prompt('Qty Mismatching', 'Enter Available QTY', function (value) {
                item.invoiceinformation = $scope.Invoice.invoiceno + ' - ' + $scope.Invoice.suppliercompanyname;
                item.actualqty = parseFloat(value);
                if (!isNaN(item.actualqty) && item.actualqty > 0) {
                    var request = {
                        "Item": item,
                        "securitytoken": "String content"
                    }
                    var srequest = JSON.stringify(request);
                    $http({
                        method: 'POST',
                        url: "LrinvoiceitemsService/UpdateActualQty",
                        data: srequest
                    }).success(function (result) {

                    });
                }
            });
        }

        $scope.Damaged = function (item) {

            if ((item.buyingqty - item.sellableqty) == 0) {
                $rootScope.Toaster.Info("Can't mark damaged/shortage after barcode generation. Please use purchase return option.");
                return;
            }
            $rootScope.Prompt('Damaged Qty', 'Enter Damaged QTY', function (value) {
                item.invoiceinformation = $scope.Invoice.invoiceno + ' - ' + $scope.Invoice.suppliercompanyname;
                item.damagedquantity = parseFloat(value);
                if (!isNaN(item.damagedquantity) && item.damagedquantity > 0) {
                    var request = {
                        "Item": item,
                        "securitytoken": "String content"
                    }
                    var srequest = JSON.stringify(request);
                    $http({
                        method: 'POST',
                        url: "LrinvoiceitemsService/Save",
                        data: srequest
                    }).success(function (result) {
                        item.damagedqty += item.damagedquantity;
                        item.damagedquantity = 0;
                        $rootScope.Toaster.Info("Debit Note Successfully created.. Please refresh the screen for changes.")
                        $scope.DefaultSearch();
                    }).error(function (result) {
                        item.damagedquantity = 0;
                        $rootScope.Toaster.Error("Error In creating Debit Note.")
                    });;
                }
            });
        }

        $scope.SizeChange = function (item) {
            var BarcodeList = [];
            var itemjson = {
                'id': item.id, 'productid': item.productid, 'qty': item.sellableqty, 'sizeid': item.Details.sizeid,
                'code': item.barcodes, 'name': $scope.SearchRef(item.productid, $scope.ProductList), 'sno': item.sno,
                'uan': item.uan
            };
            BarcodeList.push(itemjson);
            var request = {
                "Items": BarcodeList,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/SearchBarcode",
                data: srequest
            }).success(function (result) {
                var BarcodeList = [];
                $.each(result, function (k, v) {
                    BarcodeList.push({
                        'id': itemjson.id, 'productid': itemjson.productid, 'qty': v.qty, 'code': v.code,
                        'name': itemjson.name, sizeid: itemjson.sizeid, 'sno': v.sno
                    });
                });
                $scope.ShowSizeChange(item, BarcodeList);
            }).error(function (result) {
            });
        };

        $scope.ShowSizeChange = function (requestitem, BarcodeList) {
            var dialog = ngDialog.open({
                template: 'ChooseSize',
                data: {
                    'requestitem': requestitem,
                    'BarcodeList': BarcodeList,
                    'SizeList': $scope.SizeList,
                    'SearchRef': $scope.SearchRef
                },
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.BarcodeList = $scope.ngDialogData.BarcodeList
                    $scope.requestitem = $scope.ngDialogData.requestitem;
                    $scope.SizeList = $scope.ngDialogData.SizeList;

                    $scope.SelectAllChanged = function (items) {
                        $.each(items, function (k, v) {
                            v.selected = $scope.selectall;
                        });
                    };

                    $scope.Selected = function () {
                        var blist = [];
                        $.each($scope.ngDialogData.BarcodeList, function (k, v) {
                            if (v.selected) {
                                v.sizename = $scope.ngDialogData.SearchRef(v.sizeid, $scope.SizeList, '')
                                blist.push(v);
                            }
                        });
                        if (blist.length > 0)
                            $scope.closeThisDialog(blist);
                        else
                            $scope.closeThisDialog();
                    };

                    $scope.UpdateSelectedQty = function () {
                        var blist = [];
                        $.each($scope.ngDialogData.BarcodeList, function (k, v) {
                            blist.push(v);
                        });
                        if (blist.length > 0) {
                            var request = {
                                "Item": {
                                    'Core': requestitem, 'items': blist
                                },
                                "securitytoken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "LrinvoiceService/UpdateQty",
                                data: srequest
                            }).success(function (result) {

                                if (result != null &&
                                    result.length > 0) {
                                    $rootScope.Toaster.Info("Saved Successfully.!");
                                    $scope.InitializeInvoiceList(true);
                                }

                            }).error(function (result) {
                            });

                        }
                        $scope.closeThisDialog();
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        var request = {
                            "Item": {
                                'Core': requestitem, 'items': data.value, 'companyid': $scope.Invoice.companyid
                            },
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "LrinvoiceService/ChangeSize",
                            data: srequest
                        }).success(function (result) {

                            if (result != null &&
                                result.length > 0) {
                                $rootScope.Toaster.Info("Saved Successfully.!");
                                $scope.InitializeInvoiceList(true);
                            }

                        }).error(function (result) {
                        });

                    }
                }
            });
        }

        $scope.UploadBarcodes = function (value) {



            var dialog = ngDialog.open({
                template: 'UploadBarcode',
                data: {
                    'file': value,
                    'ParentScope': $scope
                },
                width: '600px',
                controller: ['$scope', '$timeout', '$parse', '$http', '$rootScope', function ($scope, $timeout, $parse, $http, $rootScope) {
                    $scope.BarcodeList = [];
                    $scope.BarcodeListColumns = [];
                    $scope.ParentScope = $scope.ngDialogData.ParentScope;
                    $scope.Buyingqty = $scope.ParentScope.GetNumber($scope.ParentScope.currentitem.buyingqty);

                    $.each($scope.ngDialogData.file.columns, function (k, v) {
                        $scope.BarcodeListColumns.push({
                            'sno': k + 1, 'name': v, 'selected': false, 'id': k
                        });
                        $scope.BarcodeListColumns[$scope.BarcodeListColumns.length - 1].selected = true;
                    });

                    $.each($scope.ngDialogData.file.rows, function (k, v) {
                        $scope.BarcodeList.push({
                            'sno': k + 1, 'codes': v.values, id: k, 'selected': false
                        });
                    });

                    $scope.ItemSelected = function (item, list) {
                        var index = item.id;
                        $.each(list, function (k, v) {
                            v.selected = false;
                            if (k >= index &&
                                k < $scope.Buyingqty + index) {
                                v.selected = true;
                            }
                        })
                    }

                    $scope.ParentScope = $scope.ngDialogData.ParentScope;
                    $scope.DeleteBarcode = function (item) {
                        $scope.BarcodeList.splice($scope.BarcodeList.indexOf(item), 1);
                        $.each($scope.BarcodeList, function (k, v) {
                            v.sno = k + 1;
                            v.id = k;
                        });
                    }

                    $scope.Save = function () {
                        var list = [];
                        var duplicatebarcode = null;
                        $.each($scope.BarcodeList, function (k, v) {
                            if (v.selected) {

                                var code = '';
                                $.each($scope.BarcodeListColumns, function (kk, vv) {
                                    if (vv.selected)
                                        code = v.codes[kk];
                                });

                                if (list.indexOf(code) >= 0)
                                    duplicatebarcode = code;
                                else
                                    list.push(code);
                            }
                        });
                        if (duplicatebarcode != null) {
                            $rootScope.Toaster.Info(duplicatebarcode + ' barcode contains multiple time');
                            return;
                        }
                        $scope.closeThisDialog(list);
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        if ($scope.currentitem != null && $scope.currentitem.id > 0 && data.value.length > 0) {

                            $scope.currentitem.barcodes = JSON.stringify(data.value);
                            $scope.currentitem.barcode = data.value.join();
                            $scope.currentitem.companyid = $scope.Invoice.companyid;

                            var request = {
                                "Item": $scope.currentitem,
                                "securitytoken": "String content"
                            }
                            $scope.currentitem.sellableqty = data.value.length;
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "LrinvoiceitemsService/UploadBarcode",
                                data: srequest
                            }).success(function (result) {

                                if (result) {
                                    $rootScope.Toaster.Info("Saved Successfully.!");
                                }
                                else {
                                    $rootScope.Toaster.Error("Cant Update Barcodes!")
                                    $scope.currentitem.barcodes = null;
                                    $scope.currentitem.barcode = null;
                                }

                            }).error(function (result) {
                            });

                        }
                    }
                }
            });
        }

        $scope.fileuploadedcomplete = function (value) {
            $scope.barcodefile = null;
            $scope.UploadBarcodes(value);
        }

        $scope.Conversion2Bit = function (item) {
            var BarcodeList = [];
            var itemjson = {
                'id': item.id, 'productid': item.productid, 'qty': item.sellableqty, 'sizeid': item.Details.sizeid,
                'code': item.barcodes, 'name': $scope.SearchRef(item.productid, $scope.ProductList), 'sno': item.sno,
                'uan': item.uan
            };
            BarcodeList.push(itemjson);
            var request = {
                "Items": BarcodeList,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/SearchBarcode",
                data: srequest
            }).success(function (result) {
                var BarcodeList = [];
                $.each(result, function (k, v) {
                    BarcodeList.push({
                        'id': itemjson.id, 'productid': itemjson.productid, 'qty': itemjson.qty, 'code': v.code,
                        'name': itemjson.name, 'sno': v.sno
                    });
                });
                $scope.ShowConversion2Bit(item, BarcodeList);
            }).error(function (result) {
            });
        };

        $scope.ShowConversion2Bit = function (requestitem, BarcodeList) {
            var dialog = ngDialog.open({
                template: 'ChooseBit',
                data: {
                    'requestitem': requestitem,
                    'BarcodeList': BarcodeList,
                    'SizeList': $scope.SizeList,
                    'SearchRef': $scope.SearchRef
                },
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.BarcodeList = $scope.ngDialogData.BarcodeList
                    $scope.requestitem = $scope.ngDialogData.requestitem;
                    $scope.SizeList = $scope.ngDialogData.SizeList;

                    $scope.SelectAllChanged = function (items) {
                        $.each(items, function (k, v) {
                            v.selected = $scope.selectall;
                        });
                    };

                    $scope.Selected = function () {
                        var blist = [];
                        $.each($scope.ngDialogData.BarcodeList, function (k, v) {
                            if (v.selected) {
                                blist.push(v);
                            }
                        });

                        if (blist.length > 0)
                            $scope.closeThisDialog(blist);
                        else
                            $scope.closeThisDialog();
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        var request = {
                            "Item": { 'Core': requestitem, 'items': data.value, 'companyid': $scope.Invoice.companyid },
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "LrinvoiceService/Change2Bit",
                            data: srequest
                        }).success(function (result) {

                            if (result != null &&
                                result.length > 0) {
                                $rootScope.Toaster.Info("Saved Successfully.!");
                            }

                        }).error(function (result) {
                        });

                    }
                }
            });
        }

        $scope.PrintPDF = function (requestitem, printdata) {
            requestitem.GenerateGUID = requestitem.IsPDF;
            requestitem.IsDesign = requestitem.IsDesign;
            var request = {
                "Item": requestitem,
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "StockService/PrintPDFBarcode",
                data: srequest
            }).success(function (result) {

                if (requestitem.GenerateGUID) {
                    $rootScope.Download('StockService/PrintPDFBarcode/' + $rootScope.UserInfo.OrganizationID + '/' + result, 'barcode.pdf', 'application/pdf');
                    return;
                    $timeout(function () {
                        var link = document.createElement('a');
                        link.download = "barcode.pdf";
                        link.href = 'StockService/PrintPDFBarcode/' + $rootScope.UserInfo.OrganizationID + '/' + result;
                        document.body.appendChild(link);
                        link.click();
                        $timeout(function () {
                            link.remove();
                        }, 100);
                    }, 10);
                }
                else {
                    $rootScope.Sent2Printer(result, printerData, true);
                }

            });
        }

        // Barcode template
        $scope.ChoseReport = function (requestitem) {
            $rootScope.Handlers.SelectPrinter('BARCODE', function (printdata) {
                if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                    return;

                requestitem.reportid = printdata.report.id;
                requestitem.IsDesign = printdata.IsDesign;
                requestitem.IsPDF = printdata.IsPDF;

                if (typeof (requestitem.IsDesign) != 'undefined' && requestitem.IsDesign) {
                    $scope.PrintPDF(requestitem, printdata)
                    return;
                }
                else if (printdata.IsPDF) {
                    var printerData = localStorageService.get('PrinterData');
                    if (printerData == null) {
                        $rootScope.Toaster.Error("Please select the printer");
                        return;
                    }
                }
                var request = {
                    "Item": requestitem,
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "StockService/PrintBarcode",
                    data: srequest
                }).success(function (result) {
                    if (result && result.ErrorMessage && result.ErrorMessage != null && result.ErrorMessage !='') {
                        $rootScope.Toaster.Error(result.ErrorMessage);
                        return;
                    }
                    if (result != null && result.length > 0) {
                        if (!requestitem.IsPDF) {
                            $rootScope.Sent2Printer(result.join(';'), printdata);
                        }
                        else {
                            $rootScope.Sent2Printer(result, printdata, 'PDF');
                        }
                    }
                });
            });
        }


        $scope.Conversion = function (ItemDetails) {
            var blist = eval(ItemDetails.barcodes);
            if (typeof (blist.length) == 'undefined' || blist.length != 1) {
                $rootScope.Toaster.Error("Single barcode only supports Conversion!");
                return;
            }

            restService.Get('SalablegoodsService/FindStock/' + blist[0]).then(function (results) {
                $scope.ConversionDialog(ItemDetails, results.wstock + results.rstock);
            });
        }

        $scope.ConversionDialog = function (ItemDetails, Total) {
            var proditem = $scope.ProductList.find(function (itm) {
                return itm.id == ItemDetails.productid;
            });

            if (proditem.sellingmode == 1)
                return;

            var isbit = proditem.sellingmode == 3;

            ItemDetails.name = proditem.name;

            var dialog = ngDialog.open({
                template: 'BitConversion',
                data: {
                    'ItemDetails': ItemDetails, 'SearchRef': $scope.SearchRef, 'scope': $scope, 'isbit': isbit, 'Total': Total
                },
                width: '680px',
                controller: ['$scope', '$timeout', 'restService', function ($scope, $timeout, restService) {
                    $scope.Created = 0;
                    $scope.CoreItem = $scope.ngDialogData.ItemDetails;
                    $scope.TotalAmount = 0;
                    $scope.ispartial = false;
                    $scope.ProductList = $scope.ngDialogData.scope.ProductList;
                    $scope.isbit = $scope.ngDialogData.isbit;
                    $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ngDialogData.ItemDetails)));
                    $scope.Total = $scope.ngDialogData.Total; //$scope.CoreItem.buyingqty - ($scope.CoreItem.damagedqty + $scope.CoreItem.returnqty);
                    $scope.PopupList = [];
                    $scope.CanClear = true;
                    $timeout(function () {
                        $scope['setFocussizeInfo']();
                    }, 100);

                    $scope.salediscountChanged = function () {
                        $scope.ItemDetails.salediscount = Math.round($scope.GetNumber($scope.ItemDetails.sellingprice) * $scope.GetNumber($scope.ItemDetails.salediscountpercentage) / 100, 0);
                    };

                    $scope.mrpChange = function () {
                        var value = $scope.ItemDetails.mrp;
                        $scope.ItemDetails.sellingprice = value;
                        $scope.ItemDetails.marginpercentage = (($scope.GetNumber($scope.ItemDetails.sellingprice) / $scope.ItemDetails.buyingprice * 100) - 100).round(2);
                        $scope.salediscountChanged();
                    }

                    $scope.CreateItems = function ($event) {
                        if (typeof ($scope.sizeInfo) == 'undefined' || $scope.sizeInfo == null || $scope.sizeInfo.toString() == '') {
                            if (!$scope.Selected()) {
                                if ($scope.PopupList.length > 0) {
                                    $scope.setFocusmcell_firstrow();
                                }
                                else {
                                    $scope['setFocussizeInfo']();
                                }
                                $event.preventDefault();
                            }
                            return true;
                        }

                        var product = $scope.ProductList.find(function (item) {
                            return item.id == $scope.ItemDetails.productid;
                        });

                        if (product != null) {
                            $scope.ItemDetails.name = product.name;
                            $scope.ItemDetails.productname = product.name;
                            $scope.ItemDetails.sellingname = product.name;
                            $scope.ItemDetails.printingname = product.name;
                        }

                        var cellid = 0;
                        var nqty = $scope.sizeInfo.toString().split('-');
                        var tqty = (nqty.length == 2 ? parseFloat(nqty[0]) : 1);
                        var meter = (nqty.length == 2 ? parseFloat(nqty[1]) : parseFloat(nqty[0]));

                        var nqty = $scope.sizeInfo.toString().split('-');
                        for (var index = 0; index < tqty; index++) {
                            var nitem = (JSON.parse(JSON.stringify($scope.ItemDetails)));
                            nitem.sellableqty = meter;
                            if ($scope.PopupList.length == 0) {
                                nitem.cellid = 'mcell_firstrow';
                            }
                            nitem.unique = (new Date().getTime());
                            $scope.PopupList.push(nitem);
                            nitem.cellid = 'mcell_' + (cellid++);
                            $scope.Created += meter;
                        }
                        $scope.Created = $scope.Created.round(2);
                        $scope.sizeInfo = null;//($scope.CoreItem.buyingqty - $scope.Created).toString();

                        $scope['setFocussizeInfo']();
                    };

                    $scope.GetNumber = function (value) {
                        if (typeof (value) == 'undefined' || value == null || isNaN(value))
                            return 0;

                        return value;
                    };

                    $scope.Delete = function (item) {
                        $scope.PopupList.splice($scope.PopupList.indexOf(item), 1);
                        $scope.Created -= item.sellableqty;
                        $scope.Created = $scope.Created.round(2);
                    };

                    $scope.Clear = function () {
                        $scope.PopupList = [];
                        $scope.Created = 0;
                        $scope.TotalAmount = 0;
                    }

                    $scope.Selected = function () {

                        if ((!$scope.ispartial && $scope.Created != $scope.Total) || $scope.Created == 'undefined' || $scope.Created == null) {
                            $rootScope.Toaster.Error("Check Qty..!");
                        }
                        else if ($scope.ispartial || $scope.Created == $scope.Total) {

                            var result = {
                                'Core': $scope.CoreItem, 'IsBit': $scope.isbit, 'IsPartial': $scope.ispartial, 'items': $scope.PopupList
                            };
                            $scope.closeThisDialog(result);
                            return true;
                        }
                        return false;
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        data.value.companyid = $scope.Invoice.companyid;
                        var request = {
                            "Item": data.value,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "LrinvoiceService/ConvertItem",
                            data: srequest
                        }).success(function (result) {

                            if (result != null &&
                                result.length > 0) {
                                $rootScope.Toaster.Info("Saved Successfully.!");
                                $scope.InitializeInvoiceList(true);
                            }

                        }).error(function (result) {
                        });
                    }
                }
            });
        };

        $scope.SearchCondition = {
            companyid: 0,
            supplierid: 0,
            invoiceno: "",
            fromdate: null,
            todate: null,
            lrentryno: ""
        }

        $scope.Search = function (autoload) {


            if (typeof (autoload) == 'undefined' && $scope.Appsetting_Company) {
                if (typeof ($scope.SearchCondition.lrid) == 'undefined'
                    || $scope.SearchCondition.lrid == null
                    || $scope.SearchCondition.lrid == '') {
                    if (typeof ($scope.SearchCondition.companyid) == 'undefined' || $scope.SearchCondition.companyid == null ||
                        $scope.SearchCondition.companyid <= 0) {
                        $rootScope.Toaster.Info("Please Select company.");
                        return;
                    }
                }
            }


            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrinvoiceService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchCondition.lrid = 0;
                if (typeof (result.Items) != 'undefined')
                    $scope.SearchResult = result.Items;
                else
                    $scope.SearchResult = result;

                if ($scope.SearchResult.length == 1 && $location.search().lrid == $scope.SearchResult[0].lrid) {
                    $scope.InvoiceInfo = {};
                    $scope.InvoiceInfo.invoiceid = $scope.SearchResult[0].id;
                    $scope.InitializeInvoiceList(true);
                }
                else {
                    $scope.ShowSearch = true;
                    $timeout(function () {
                        $scope.BarcodeConfig();
                    }, 50);
                }
            }).error(function (result) {
            });
        }

        $scope.BarcodeConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);

            igrid.AddReferenceColumn('suppliercompanyid', 'Supplier', 200, $scope.SupplierCompany);
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 120);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('baseamount', 'Base Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxamount', 'Tax Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totalamount', 'TotalAmount', 110, 'N2', 2).total = true;

            var commands = "";
            if ($rootScope.UserInfo.p_BARCODEGEN.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_BARCODEGEN.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_BARCODEGEN.modify || $rootScope.UserInfo.p_BARCODEGEN.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 130, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                    });
                });

            }
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult, '_id');

        }

        $scope.EntryGridConfig = function () {
            if ($scope.egrid != null)
                $scope.egrid.destroy();

            var igrid = new InfyGrid('gridList', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 40);
            var column = igrid.AddCustomColumn('Name', 'Product Name', 100, function (item, col) {
                return $scope.SearchProductRef(item);
            });

            if ($scope.system_invcol_brand) {
                var column = igrid.AddCustomColumn('brandid', 'Brand', 50, function (item, col) {
                    return $scope.SearchRefDirect(item.Details.brandid, $scope.BrandName);
                });
                column.enablefilter = true;
            }

            var column = igrid.AddCustomColumn('sizeid', 'Size', 50, function (item, col) {
                return $scope.SearchRefDirect(item.Details.sizeid, $scope.SizeList);
            });
            column.enablefilter = true;

            igrid.AddSimpleColumn('designid', 'Design', 80);
            var column = igrid.AddCustomColumn('buyingqty', 'Qty', 50, function (item, col) {
                if (item.returnqty > 0)
                    return item.buyingqty.toString() + "(" + item.actualqty + ")";
                if (item.actualqty > 0 && item.buyingqty != item.actualqty)
                    return item.actualqty.toString() + "(" + item.buyingqty + ")";
                return item.buyingqty.toString();
            });
            column.cssClass = 'rightalign';
            column.enablefilter = true;

            if (!$scope.HideCostingValue) {
                var column = igrid.AddCustomColumn('buyingqty', 'Cost', 50, function (item, col) {
                    if (item.basebuyingprice != item.buyingprice)
                        return item.basebuyingprice.toString() + "/" + item.buyingprice;
                    return item.buyingprice;
                });
                column.cssClass = 'rightalign';
                igrid.AddFormatedColumn('marginpercentage', 'Margin', 50, 'N2', 2).total = true;
            }

            if (!$scope.HideSellingValue) {
                if ($scope.CanChangePrice) {
                    var column = igrid.AddColumn(
                        {
                            id: "sellingprice", name: "Price", field: "sellingprice",
                            width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                            'sortable': true
                        }
                    );
                    column.cssClass = 'rightalign';
                    var column = igrid.AddColumn(
                        {
                            id: "salediscountpercentage", name: "Disc%", field: "salediscountpercentage",
                            width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                            'sortable': true
                        }
                    );
                    column.cssClass = 'rightalign';
                    var column = igrid.AddColumn(
                        {
                            id: "salediscount", name: "Disc", field: "salediscount",
                            width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                            'sortable': true
                        }
                    );
                    column.cssClass = 'rightalign';
                    var column = igrid.AddColumn(
                        {
                            id: "mrp", name: "MRP", field: "mrp",
                            width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                            'sortable': true
                        }
                    );
                    column.cssClass = 'rightalign';
                    var column = igrid.AddColumn(
                        {
                            id: "finalprice", name: "Final", field: "finalprice",
                            width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                            'sortable': true
                        }
                    );
                    column.cssClass = 'rightalign';
                }
                else {
                    var column = igrid.AddFormatedColumn('sellingprice', 'Price', 60, 'N2', 2);
                    column.cssClass = 'rightalign';
                    var column = igrid.AddFormatedColumn('salediscount', 'Discount', 60, 'N2', 2);
                    column.cssClass = 'rightalign';
                    var column = igrid.AddFormatedColumn('mrp', 'MRP', 60, 'N2', 2);
                    column.cssClass = 'rightalign';
                    var column = igrid.AddCustomColumn('finaldiscount', 'Discount', 60, function (item, col) {
                        return item.salediscountpercentage.toString() + "% @ " + item.salediscount;
                    });
                    column.cssClass = 'rightalign';
                    var column = igrid.AddCustomColumn('finalprice', 'Final', 60, function (item, col) {
                        return item.sellingprice - item.salediscount;
                    });
                    column.cssClass = 'rightalign';
                }
            }
            else {
                var column = igrid.AddCustomColumn('brandid', 'Brand', 100, function (item, col) {
                    return $scope.SearchRefDirect(item.Details.brandid, $scope.BrandName);
                });
                var column = igrid.AddCustomColumn('colourid', 'Colour', 100, function (item, col) {
                    return $scope.SearchRefDirect(item.Details.colourid, $scope.ColorList);
                });
                var column = igrid.AddCustomColumn('styleid', 'Style', 100, function (item, col) {
                    return $scope.SearchRefDirect(item.Details.styleid, $scope.StyleList);
                });
            }

            if ($scope.SupportPriceTags) {
                var column = igrid.AddCustomColumn('priceTag', 'Tag', 50, function (item, col) {
                    if (item.Details && item.Details.pricetagid && item.Details.pricetagid > 0)
                        return $scope.SearchRef(item.Details.pricetagid, $scope.PriceTagList, '');
                    return '';
                });
            }


            if ($scope.SupportPriceTypes &&
                $scope.PriceTags.length > 0) {
                $.each($scope.PriceTags, function (k, v) {
                    column = igrid.AddFormatedColumn('pt_' + v.id, v.name, 60, 'N2', 2);
                    column.editor = $scope.CanChangePrice ? Slick.Editors.Text : null;
                    column.cssClass = 'rightalign';
                    column.internal = v;
                    column.total = true;
                    column.getValue = function (item, col) {
                        if (item.Details.pricelist && item.Details.pricelist.length > 0) {
                            var value = item.Details.pricelist.find(function (item) {
                                return item.id == col.internal.id;
                            });
                            if (typeof (value) == 'undefined' || value == null)
                                return 0;
                            return value.price;
                        }
                        return 0;
                    };
                    column.formatter = function (row, cell, searchValue, columnDef, dataContext) {
                        if (dataContext.Details.pricelist && dataContext.Details.pricelist.length > 0) {
                            var value = dataContext.Details.pricelist.find(function (item) {
                                return item.id == columnDef.internal.id;
                            });
                            if (typeof (value) == 'undefined' || value == null)
                                return 0;
                            return value.price;
                        }
                        return 0;
                    }
                });
            }

            igrid.AddFormatedColumn('sellableqty', 'Gnrd', 60, 'N0', 0);
            //igrid.AddColumn(
            //    {
            //        id: "newqty", name: "Barcode", field: "newqty",
            //        width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
            //        'sortable': true
            //    }
            //);
            //column.cssClass = 'rightalign';



            var column = igrid.AddCustomColumn('barcode', 'Barcode', 50, function (item, col) {
                if (item.sellableqty > 0 && item.uan && item.uan != null && item.uan != '')
                    return item.uan;
                return item.barcode;
            });
            column.getValue = function (item, col) {
                if (item.sellableqty > 0 && item.uan && item.uan != null && item.uan != '')
                    return item.uan;
                return item.barcode;
            };
            column.enablefilter = true;

            if ($rootScope.UserInfo.p_BARCODEGEN.modify) {
                var commands = "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
                igrid.AddButtonColumn('action', 'ACTION', 60, commands, function (e, args) {
                    $scope.$apply(function () {
                        $scope.MapProduct(args.item);
                    });
                });
            }

            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.egrid = igrid;

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                if (args.grid.getColumns()[args.cell].id == 'mrp') {
                    $scope.MRPChanged(item);
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
                if (args.grid.getColumns()[args.cell].id == 'sellingprice') {
                    $scope.PriceChanged(item);
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }

                else if (args.grid.getColumns()[args.cell].id == 'salediscountpercentage') {
                    $scope.salediscountPercentageChanged(item);
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
                else if (args.grid.getColumns()[args.cell].id == 'salediscount') {
                    $scope.salediscountChanged(item);
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
                else if (args.grid.getColumns()[args.cell].id == 'finalprice') {
                    $scope.finalpriceChanged(item);
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                } else if (args.grid.getColumns()[args.cell].id.startsWith('pt_')) {

                    if (typeof (item.Details.pricelist) == 'undefined' || item.Details.pricelist == null) {
                        item.Details.pricelist = [];
                        $.each($scope.PriceTags, function (k, v) {
                            item.Details.pricelist.push({ 'id': v.id, 'price': item.Details.sellingprice });
                        })
                    }

                    var pricetagid = args.grid.getColumns()[args.cell].internal.id;
                    var priceitem = item.Details.pricelist.find(function (si) {
                        return si.id == pricetagid
                    });
                    priceitem.price = item[args.grid.getColumns()[args.cell].id];

                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });

            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    var selectedrows = $scope.egrid.GetSelectedRows();
                    $scope.SelectedRowCount = selectedrows.length;
                    $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                        var selected = selectedrows.find(function (item) {
                            return item.id == v.id;
                        });
                        if (!selected || selected == null || v.SelectedItem) {
                            v.SelectedItem = false;
                            $scope.Changprintingeqty(v);
                        }
                    });
                    $.each($scope.egrid.GetSelectedRows(), function (k, v) {
                        v.SelectedItem = true;
                        $scope.Changprintingeqty(v);
                    });
                }, 10);
            };

            $scope.egrid.UpdateData([]);
        }

        $scope.SelectedRowCount = 0;

        $scope.ShowMarkerDialog = function () {
            var seletedRows = $scope.egrid.GetSelectedRows();
            if (seletedRows == 'undefined' || seletedRows == null || seletedRows.length == 0) {
                $rootScope.Toaster.Info("Please select the items to be added/removed.");
                return;
            }

            var dialog = ngDialog.open({
                template: 'ChangeStockMarker',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', 'restService',
                    function ($scope, $timeout, restService) {
                        $scope.parent = $scope.ngDialogData[0];

                        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
                            $scope.Locations = results;
                        });

                        $scope.MarkerType = restService.Get('ReferencelistService/FULL-MARKER', true).then(function (results) {
                            $scope.MarkerType = results;
                        });

                        $scope.AddFromList = function () {
                            var marker = $scope.MarkerType.find(function (item) {
                                return item.id == $scope.markerid;
                            });
                            $scope.closeThisDialog({
                                'action': 'Add', 'marker': marker,
                                'markerid': $scope.markerid, 'locationid': $scope.locationid
                            });
                        };
                        $scope.RemoveFromList = function () {
                            $scope.closeThisDialog({
                                'action': 'Delete', 'marker': marker,
                                'markerid': $scope.markerid, 'locationid': $scope.locationid
                            });
                        };
                        $scope.DeleteAll = function () {
                            $scope.closeThisDialog({
                                'action': 'DeleteAll', 'marker': marker,
                                'markerid': $scope.markerid, 'locationid': $scope.locationid
                            });
                        };
                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {

                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        var result = data.value;

                        if (result.markerid == null || result.markerid == 'undefined' || result.markerid == 0) {
                            $rootScope.Toaster.Info("Please select the marker name");
                            return;
                        }

                        var request = {
                            "items": [], "delete": result.action == "Delete",
                            "deleteall": result.action == "DeleteAll",
                            "isinvoice": true, "invoiceitemids": []
                        };

                        var seletedRows = $scope.egrid.GetSelectedRows();

                        $.each(seletedRows, function (k, v) {
                            request.invoiceitemids.push(v.id);
                        });

                        var webrequest = {
                            "item": request,
                            "securitytoken": "String content"
                        }

                        request.locationid = result.locationid;
                        request.markerid = result.markerid;
                        request.isdamaged = result.marker.additionalinfo.disabled;
                        request.isonline = result.marker.additionalinfo.isonline;

                        var srequest = JSON.stringify(webrequest);
                        $http({
                            method: 'POST',
                            url: "StockmarkerService/Save",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info("Saved Successfully.");
                        }).error(function (result) {
                        });

                    }

                }
            });
        }

        $timeout(function () {
            $scope.LoadApplicationSetting();
            //$scope.setFocusfirst();
        }, 10);

        $scope.Appsetting_Company = false;
        $scope.PriceTags = [];

        $scope.LoadApplicationSetting = function () {
            if (typeof ($rootScope.UserInfo) != 'undefined' &&
                typeof ($rootScope.UserInfo.appsettings) != 'undefined') {
                $scope.HideSellingValue = $rootScope.UserInfo.p_SHOWSELLINGVALUE && $rootScope.UserInfo.p_SHOWSELLINGVALUE.view;
                $scope.HideCostingValue = $rootScope.UserInfo.p_SHOWVALUE && $rootScope.UserInfo.p_SHOWVALUE.view;
                $scope.Appsetting_Company = true;
                restService.Get('ReferencelistService/PRICETAGS', true).then(function (results) {
                    $.each(results, function (k, v) {
                        if (v.additionalinfo.billdiscountinclude)
                            $scope.PriceTags.push(v);
                    });
                    $scope.EntryGridConfig();
                    $scope.DefaultSearch();
                });

            }
            else {
                $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50);
            }
        }

        $scope.BuildSKUDesign = function () {
            var stocklist = [];
            $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                if (typeof (v.SelectedItem) != 'undefined' && v.SelectedItem) {
                    stocklist.push(v.id);
                }
            });

            request = {
                "Item": { ids: stocklist },
                "SecurityToken": "String content"
            }

            $http({
                method: 'POST',
                url: "LrinvoiceitemsService/CreateB2BSKU",
                data: request
            }).success(function (result) {

            });
        }

        $scope.BuildImageGrid = function () {
            var stocklist = [];
            $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                if (typeof (v.SelectedItem) != 'undefined' && v.SelectedItem) {
                    stocklist.push(v.id);
                }
            });

            request = {
                "Item": { ids: stocklist },
                "SecurityToken": "String content"
            }

            $http({
                method: 'POST',
                url: "LrinvoiceitemsService/SearchBarcodeImages",
                data: request
            }).success(function (result) {

            });
        }

    }]);
