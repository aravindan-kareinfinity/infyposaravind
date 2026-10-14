var app = angular.module('InfinityLab');

app.controller('ReportFormatController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.panelheight = { 'height': '500px' };
        $scope.canvasheight = { 'height': '500px' };

        $scope.SearchRef = restService.SearchReference;
        $scope.ShowTotalLines = false;
        $scope.ShowPaperSize = false;
        $scope.ShowDesigner = false;
        $scope.refreportlist = [
            { 'code': 'SALERECEIPT', 'value': 'Sale Receipt', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true, 'ShowPaperSize': true },
            { 'code': 'SALERECEIPTPDF', 'value': 'Sale Receipt in PDF', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'SALEINVOICE', 'value': 'B2B Sale Invoice', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'SALEINVOICEDISC', 'value': 'B2B Sale Invoice - Discount', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'SALERECEIPTCUSTOM', 'value': 'Sale Receipt - Custom', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true, 'ShowPaperSize': true },
            { 'code': 'SALERECEIPTGST', 'value': 'Sale Receipt - GST', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true, 'ShowPaperSize': true },
            { 'code': 'TOUCHRECEIPT', 'value': 'Touch Sale Receipt', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true, 'ShowPaperSize': true },
            { 'code': 'ALTERATION', 'value': 'Alteration', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true, 'ShowPaperSize': true },
            { 'code': 'GIFTDELIVERY', 'value': 'Gift Delivery', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true, 'ShowPaperSize': true },
            { 'code': 'GIFTCOUPON', 'value': 'Gift Coupon', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true, 'ShowPaperSize': true },
            { 'code': 'GIFTBARCODE', 'value': 'Gift Coupon Barcode', 'ShowTotalLines': true, 'SupportInstance': true, 'XML': false, 'ShowPaperSize': true },
            { 'code': 'CUTTINGBARCODE', 'value': 'Cutting Barcode', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true, 'ShowPaperSize': true },
            { 'code': 'SALERETURNINVOICE', 'value': 'B2B Sale Return Invoice', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'SALERETURNINVOICEDISC', 'value': 'B2B Sale Return Invoice - Discount', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'B2BDELIVERYCHALLAN', 'value': 'B2B Delivery Slip', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'CHEQUESUMMERY', 'value': 'Cheque Summary', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'SETTLEMENT', 'value': 'Settlement Slip', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true, 'ShowPaperSize': true },
            { 'code': 'SALERETURN', 'value': 'Sale Return Receipt', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true, 'ShowPaperSize': true },
            { 'code': 'DELIVERYCHALLAN', 'value': 'Delivery Challan', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'ESTIMATEBILL', 'value': 'Estimate Bill', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'CUSTOMERRECEIPT', 'value': 'Credit Customer Payment Receipt', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'CUSTOMERRECEIPTPDF', 'value': 'Credit Customer Payment Receipt in PDF', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'CUSTOMERORDER', 'value': 'Customer Order Receipt', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'CUSTOMERORDERPDF', 'value': 'Customer Order Receipt in PDF', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'DEBITNOTE', 'value': 'Debit Note', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'COURIER', 'value': 'Courier Receipt', 'ShowTotalLines': true, 'SupportInstance': false, 'XML': true },
            { 'code': 'CREDITNOTE', 'value': 'Supplier Credit Note', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'STOCKDUMP', 'value': 'Stock Dumping', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'CUSTOMERCREDITNOTE', 'value': 'Customer Credit Note', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'CUSTOMERLABEL', 'value': 'Customer Label', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'SUPPLIERLABEL', 'value': 'Supplier Label', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'GIFTVOUCHERPDF', 'value': 'Gift Voucher PDF', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'LEADFORMPDF', 'value': 'Lead Form in PDF', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'PO-SIZE', 'value': 'Purchase Order - Size Wise', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'PO-ITEM', 'value': 'Purchase Order - Item Wise', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'PO-DESIGN', 'value': 'Purchase Order - Design Wise', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'PO-CONSUMABLES', 'value': 'Purchase Order - Consumables', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },

            { 'code': 'SO-SIZE', 'value': 'Sales Order - Size Wise', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'SO-ITEM', 'value': 'Sales Order - Item Wise', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'SO-SLIP', 'value': 'Sales Order - Size Wise SLIP', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'COMPPO', 'value': 'Component Purchase Order', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },

            { 'code': 'LRBUNDLELABEL', 'value': 'LR Bundle Sticker', 'ShowTotalLines': true, 'SupportInstance': false, 'XML': false },
            { 'code': 'BARCODE', 'value': 'BarCode', 'ShowTotalLines': true, 'SupportInstance': false, 'XML': false },
            { 'code': 'DISCOUNT', 'value': 'Discount Slip', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'APPLICATIONFORM', 'value': 'Application Form', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'EMPLOYEECODE', 'value': 'Employee Code', 'ShowTotalLines': true, 'SupportInstance': false, 'XML': true },
            { 'code': 'SALESUMMARYSINGLERECEIPT', 'value': 'Sale Receipt - Single Bill (Multi Company)', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true, 'ShowPaperSize': true },
            { 'code': 'SALESUMMARYRECEIPT', 'value': 'Sale Receipt - Summary', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true, 'ShowPaperSize': true },
            { 'code': 'CHEQUEADVICE', 'value': 'Supplier Payment Advice Slip', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'TRANSPORTPAYMENT', 'value': 'Transport Payment Advice Slip', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'COURIERPAYMENT', 'value': 'Courier Payment Advice Slip', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'CHEQUEPRINT', 'value': 'Cheque Print', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'VOUCHERRECEIPT', 'value': 'Voucher Receipt', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'COUPONPRINT', 'value': 'Coupon Print', 'ShowTotalLines': true, 'SupportInstance': false, 'XML': false },
            { 'code': 'STOCKTRANSFER', 'value': 'Stock Transfer Print', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'PERMISSIONSLIP', 'value': 'Permission Slip', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'LEAVESLIP', 'value': 'Leave Slip', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'PURCHASEINVOICEDCPNY', 'value': 'Vendor Discrepancy', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'PURCHASEINVOICE', 'value': 'Vendor Invoice', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'PURCHASEINVOICEDETAILS', 'value': 'Vendor Invoice Details', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'JOBWORKBILL', 'value': 'Job Work Bill', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'LEDGER', 'value': 'Ledger', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'PETTYCASH', 'value': 'Petty Cash', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true},
            { 'code': 'BOMSTOCKTRANSFEROUT', 'value': 'BOM Transfer Print - Out', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },
            { 'code': 'BOMSTOCKTRANSFERIN', 'value': 'BOM Transfer Print -In', 'ShowTotalLines': false, 'SupportInstance': true, 'XML': true },

            { 'code': 'JOBWORK-ISSUE', 'value': 'Job Order - ISSUE', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'JOBWORK-RETURN', 'value': 'Job Order - RETURN', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'WORKORDER', 'value': 'Work Order', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'WORKORDERGRN', 'value': 'Work Order - GRN', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'SAMPLEORDER', 'value': 'Sample Order', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'DELIVERYPERSONSLIP', 'value': 'Delivery Person Slip', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'COMPONENTDC', 'value': 'BOM Delivery Challon', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'COMPONENTSTOCKDC', 'value': 'BOM DC Stock Summary', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'BOMSTOCKSUMMARY', 'value': 'BOM Stock Summary', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'BOMINVOICE', 'value': 'BOM Invoice', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'BOMDEBITNOTE', 'value': 'BOM Debit Note', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'BOMCREDITNOTE', 'value': 'BOM Credit Note', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'TAILORING', 'value': 'Tailoring', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'SWATCHFOLDER', 'value': 'Swatches And Folder', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'SAMPLEPRINT', 'value': 'Samples issue/receipt', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'TAILORINGREPORT', 'value': 'Tailoring Report', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'TAILORINGPAYMENT', 'value': 'Tailoring Payment', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'PACKINGSLIP', 'value': 'Packing Slip', 'ShowTotalLines': false, 'SupportInstance': false, 'XML': true },
            { 'code': 'WITOKEN', 'value': 'Walk In Token', 'ShowTotalLines': true, 'SupportInstance': false, 'XML': true },
            { 'code': 'RFTOKEN', 'value': 'Referral Token', 'ShowTotalLines': true, 'SupportInstance': false, 'XML': true }
        ];

        $scope.NOOFLINES = [{ 'id': 0, 'name': 'Default' },
            { 'id': 4, 'name': '4 Inches' },
            { 'id': 3, 'name': '3 Inches' }];

        $scope.CompanyList = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.CompanyList = results;
        });

        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.ReportChanged = function () {
            var report = $scope.refreportlist.find(function (item) {
                return item.code == $scope.Configuration.key;
            });
            /*  $scope.ShowOrg = report != null && report.ShowOrg;
              $scope.ShowName = report != null && report.ShowName;*/
            $scope.ShowTotalLines = report != null && report.ShowTotalLines;
            $scope.SupportInstance = report != null && report.SupportInstance;
            $scope.SupportPaperSize = report != null && report.ShowPaperSize;
        }

        $scope.Entity = {
            "id": 0,
            "userid": 0,
            "key": "",
            "reportformat": { format: "" },
            "organizationid": "",
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": new Date(),
            "modifiedon": new Date(),
            "name": "",
            "nooflines": 0,
            "isdefault": false,
            "locationid": ""
        }

        $scope.ShowSearch = true;
        $scope.SearchResult = [];
        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }
        $scope.New = function () {
            $scope.Configuration = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ShowSearch = false;
        }

        $scope.Configuration = (JSON.parse(JSON.stringify($scope.Entity)));

        if (typeof ($scope.Configuration.organizationid) == 'undefined' || $scope.Configuration.organizationid == 0) {
            $scope.SwitchPanel();
        }
        else {
            // $scope.InitializeInvoiceList();
        }

        $scope.SearchCondition = {
            organizationid: "",
            institutionid: UserDetails.institutionid,
            userid: 0,
            name: "",
            key: "",
            locationid: ""
        }
        $scope.Search = function () {

            /* if ($scope.Configuration.key == null ||
                $scope.Configuration.key == '')
                return;

                 if ($scope.ShowOrg) {
                if (typeof ($scope.Configuration.organizationid) == 'undefined' ||
                $scope.Configuration.organizationid == null)
                    return;

                $scope.SearchCondition.organizationid = $scope.Configuration.organizationid;
            }
            else
            {
                $scope.Configuration.organizationid = 0;
            }

            $scope.SearchCondition.key = $scope.Configuration.key;*/

            $scope.SearchCondition.excludekeys = ['WGCONFIG'];
            request = {
                "Item": $scope.SearchCondition,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ConfigurationService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {
                    $scope.SearchResult = result;
                    $.each($scope.SearchResult, function (k, v) {
                        var detail = $scope.refreportlist.find(function (item) { return item.code == v.key; });
                        if (detail && detail != null)
                            v.typedisplay = detail.value;
                    })
                    //$scope.Configuration = result[0];
                }
                else {
                    $scope.Configuration = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.Configuration.key = $scope.SearchCondition.key;
                    $scope.SearchResult = [];
                }
            }).error(function (result) {
            });

        }

        $scope.PrintBarcode = function () {

            request = {
                "Item": {
                    'format': $scope.Configuration.reportformat.format,
                    'nooflines': $scope.Configuration.nooflines,
                    'noofcopy': $scope.Configuration.noofcopy,
                    'displayvalue': $scope.Configuration.displayvalue,
                    'barcodevalue': $scope.Configuration.barcodevalue
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ConfigurationService/Print",
                data: srequest
            }).success(function (result) {

                if (result != null && result.printdocument.length > 0) {
                    var printerData = localStorageService.get('PrinterData');
                    $scope.Sent2Printer(result.printdocument, printerData);
                }

            }).error(function (result) {
            });
        };

        $scope.AddFont = function (name, filename) {
            var font = new fabric.nodeCanvas.Font(name, filename);
            font.addFace(__dirname + '/fonts/Ubuntu-Bold.ttf', 'bold');
            font.addFace(__dirname + '/fonts/Ubuntu-Italic.ttf', 'normal', 'italic');
            font.addFace(__dirname + '/fonts/Ubuntu-BoldItalic.ttf', 'bold', 'italic');

            canvas.contextContainer.addFont(font);  // when using createPNGStream or createJPEGStream

            var text = new fabric.Text('regular', {
                left: 150,
                top: 50,
                fontFamily: 'Ubuntu'
            });
            canvas.add(text);

            text = new fabric.Text('bold', {
                left: 150,
                top: 100,
                fontFamily: 'Ubuntu',
                fontWeight: 'bold'
            });
            canvas.add(text);

            text = new fabric.Text('italic', {
                left: 150,
                top: 150,
                fontFamily: 'Ubuntu',
                fontStyle: 'italic'
            });
            canvas.add(text);

            text = new fabric.Text('bold italic', {
                left: 150,
                top: 200,
                fontFamily: 'Ubuntu',
                fontWeight: 'bold',
                fontStyle: 'italic'
            });
            canvas.add(text);
        }

        $scope.SetCanvasSize = function (CanvasHeight, CanvasWidth) {
            CanvasHeight = parseFloat(CanvasHeight);
            CanvasWidth = parseFloat(CanvasWidth);
            $scope.CanvasHeight = (CanvasHeight / 0.26458333333333334);
            $scope.CanvasWidth = (CanvasWidth / 0.26458333333333334);
            $scope.canvas.setHeight($scope.CanvasHeight);
            $scope.canvas.setWidth($scope.CanvasWidth);
            $scope.canvas.renderAll();
            //millimeters.width = Math.floor(width * 0.26458333333333334);
            /*FromPresentation(1)
    Centimeter: 0.026458333333333334
    Inch: 0.010416666666666666
    Millimeter: 0.26458333333333334
    Point: 0.75
    Presentation: 1
    Type: Presentation
    Value: 1*/
        }

        $scope.getActiveProp = function (name) {
            var object = $scope.canvas.getActiveObject();
            if (!object) return '';

            return object[name] || '';
        }

        $scope.SetPosition = function (name, value) {
            var f = parseFloat(value);
            if (isNaN(f)) return;
            $scope.setActiveProp(name, f);
        }
        $scope.setActiveProp = function (name, value) {
            var object = $scope.canvas.getActiveObject();
            if (!object) return;
            object.set(name, value).setCoords();
            $scope.canvas.renderAll();
        }

        $scope.ImageChanged = function (id) {
            var activeObject = $scope.canvas.getActiveObject();
            if (activeObject != null) {
                activeObject.setSrc('File/Show/' + $scope.UserInfo.OrganizationID + '/' + id,
                    function () {
                        $scope.canvas.renderAll();
                    });
            }
        }

        $scope.SaveDesigner = function () {
            $scope.canvas.setHeight($scope.CanvasHeight);
            $scope.canvas.setWidth($scope.CanvasWidth);
            $scope.canvas.renderAll();
            var json_data = JSON.stringify($scope.canvas.toDatalessJSON(['width', 'height']), null, '\t');
            $scope.Configuration.reportformat.format = json_data;
            $scope.ShowDesigner = false;
        }

        $scope.FieldChanged = function () {
            $scope.setActiveProp('text', $scope.Designer.FieldID);
            $scope.setActiveProp('field', $scope.Designer.FieldID);
        }

        $scope.FontChanged = function () {
            $scope.setActiveProp('fontFamily', $scope.Designer.FontID);
        }

        

        $scope.ZoomChanged = function () {
            var fsize = parseInt($scope.Designer.ZoomValue, 10);
            if (isNaN(fsize) || fsize == 0) {
                $scope.canvas.setZoom(1);
                $scope.canvas.setHeight($scope.CanvasHeight);
                $scope.canvas.setWidth($scope.CanvasWidth);
                $scope.canvas.renderAll();
            }
            else {
                $scope.canvas.setHeight($scope.CanvasHeight * fsize / 100);
                $scope.canvas.setWidth($scope.CanvasWidth * fsize / 100);
                $scope.canvas.renderAll();
                $scope.canvas.setZoom(fsize / 100);
            }
        }

        $scope.ChangeFontSize = function () {
            var fsize = parseInt($scope.Designer.FontSize, 10);
            if (!isNaN(fsize))
                $scope.setActiveStyle('fontSize', fsize);

            $scope.setActiveStyle('fontWeight', $scope.Designer.Bold ? 'bold' : 'normal');
            $scope.setActiveStyle('fontStyle', $scope.Designer.Italic ? 'italic' : 'normal');
        }

        $scope.getActiveStyle = function (styleName, object) {
            object = object || $scope.canvas.getActiveObject();
            if (!object) return '';

            return (object.getSelectionStyles && object.isEditing)
                ? (object.getSelectionStyles()[styleName] || '')
                : (object[styleName] || '');
        };

        $scope.getStyle = function (styleName, object) {
            object = object || $scope.canvas.getActiveObject();
            if (!object) return '';
            return (object.getSelectionStyles && object.isEditing)
                ? object.getSelectionStyles()[styleName]
                : object[styleName];
        }

        $scope.setActiveStyle = function (styleName, value, object) {
            object = object || $scope.canvas.getActiveObject();
            if (!object) return;

            if (object.setSelectionStyles && object.isEditing) {
                var style = {};
                style[styleName] = value;
                object.setSelectionStyles(style);
                object.setCoords();
            }
            else {
                object.set(styleName, value);
            }

            object.setCoords();
            $scope.canvas.requestRenderAll();
        };


        $scope.sendBackwards = function () {
            var activeObject = $scope.canvas.getActiveObject();
            if (activeObject) {
                $scope.canvas.sendBackwards(activeObject);
            }
        };

        $scope.sendToBack = function () {
            var activeObject = $scope.canvas.getActiveObject();
            if (activeObject) {
                $scope.canvas.sendToBack(activeObject);
            }
        };

        $scope.bringForward = function () {
            var activeObject = $scope.canvas.getActiveObject();
            if (activeObject) {
                $scope.canvas.bringForward(activeObject);
            }
        };

        $scope.bringToFront = function () {
            var activeObject = $scope.canvas.getActiveObject();
            if (activeObject) {
                $scope.canvas.bringToFront(activeObject);
            }
        };



        $scope.Designer = {
            'id': 1, 'currentid': -1, 'showfill': false, 'showfont': false, 'showdelete': false, 'showfield': false, 'showupload': false, 'type': null, 'controls': []
        };

        $scope.WireControlEvents = function (object) {
            var id = $scope.Designer.id++;
            object.controlid = id;
            $scope.Designer.controls[id] = object;
        }

        $scope.AddControl = function (name) {
            var object = null;
            switch (name) {
                case 'Barcode':
                    fabric.Image.fromURL('clientside/dist/img/barcode.png', function (image) {
                        image.set({
                            left: 0,
                            top: 0
                        }).scale(1).setCoords();
                        image.toObject = (function (toObject) {
                            return function () {
                                return fabric.util.object.extend(toObject.call(this), {
                                    controltype: this.controltype,
                                    controlid: this.controlid,
                                    field: this.field
                                });
                            };
                        })(image.toObject);

                        image.controltype = 'Barcode';
                        $scope.canvas.add(image);
                        $scope.WireControlEvents(image);
                    });
                    break;
                case 'QR':
                    fabric.Image.fromURL('clientside/dist/img/qrcode.png', function (image,option) {
                        image.set({
                            left: 0,
                            top: 0
                        }).scale(1).setCoords();

                        image.toObject = (function (toObject) {
                            return function () {
                                return fabric.util.object.extend(toObject.call(this), {
                                    controltype: this.controltype,
                                    controlid: this.controlid,
                                    field: this.field
                                });
                            };
                        })(image.toObject);
                        image.controltype = 'QR';
                        $scope.canvas.add(image);
                        $scope.WireControlEvents(image);
                    });
                    break;
                case 'Text':
                    object = new fabric.Textbox('Sample Text', {
                        left: 0,
                        top: 0,
                        width: 150,
                        fontSize: 12,
                        fontStyle: 'normal',
                        fontWeight: 'normal',
                        fontFamily: 'arial',
                        noScaleCache: false
                    });
                    break;
                case 'Field':
                    object = new fabric.Text('Product', {
                        left: 0,
                        top: 0,
                        width: 150,
                        fontSize: 12,
                        fontStyle: 'normal',
                        fontWeight: 'normal',
                        fontFamily: 'arial',
                        noScaleCache: false
                    });
                    break;
                case 'Rectangle':
                    object = new fabric.Rect({
                        left: 0,
                        top: 0,
                        width: 50,
                        height: 50,
                        fill: 'black',
                        stroke: 'black',
                        strokeWidth: 1,
                        angle: 0,
                        padding: 0,
                        noScaleCache: false
                    });
                    break;
                case 'Triangle':
                    object = new fabric.Triangle({
                        left: 0,
                        top: 0,
                        width: 100,
                        height: 100,
                        fill: 'black',
                        padding: 0,
                        noScaleCache: false
                    });
                    break;
                case 'Circle':
                    object = new fabric.Circle({
                        left: 0,
                        top: 0,
                        strokeWidth: 1,
                        radius: 12,
                        fill: 'black',
                        stroke: 'black',
                        noScaleCache: false
                    });
                    break;
                case 'HLine':
                    object = new fabric.Line([10, 10, 50, 10], {
                        fill: 'black',
                        stroke: 'black',
                        strokeWidth: 1,
                        noScaleCache: false
                    });
                    break;
                case 'VLine':
                    object = new fabric.Line([10, 10, 10, 50], {
                        fill: 'black',
                        stroke: 'black',
                        strokeWidth: 1,
                        noScaleCache: false
                    });
                    break;
                case 'Image':
                    fabric.Image.fromURL('clientside/dist/img/man.png', function (image) {
                        image.set({
                            left: 0,
                            top: 0
                        })
                            .scale(1)
                            .setCoords();

                        $scope.canvas.add(image);
                        $scope.WireControlEvents(image);
                        image.controltype = 'Image';
                    });
                    break;
            }
            if (object != null) {

                object.toObject = (function (toObject) {
                    return function () {
                        return fabric.util.object.extend(toObject.call(this), {
                            controltype: this.controltype,
                            controlid: this.controlid,
                            field: this.field
                        });
                    };
                })(object.toObject);

                object.controltype = name;
                $scope.canvas.add(object);
                $scope.WireControlEvents(object);
            }
        }



        $scope.SetActiveControl = function (object) {
            $scope.Designer.currentid = 0;
            $scope.Designer.type = '';
            //object = object || $scope.canvas.getActiveObject();
            if (object) {
                $scope.Designer.currentid = object.controlid;
                $scope.Designer.type = object.controltype;
                $scope.Designer.showdelete = true;
                $scope.Designer.activeleft = object.get('left');
                $scope.Designer.activetop = object.get('top');
                $scope.Designer.activewidth = object.get('width');
                $scope.Designer.activeheight = object.get('height');
            }
            else {
                $scope.Designer.currentid = 0;
                $scope.Designer.type = '';
                $scope.Designer.showdelete = false;
            }
            if ($scope.Designer.type == 'Text' || $scope.Designer.type == 'Field') {
                $scope.Designer.FontSize = $scope.getStyle('fontSize');
                $scope.Designer.Bold = $scope.getStyle('fontWeight') != 'normal';
                $scope.Designer.Italic = $scope.getStyle('fontStyle') != 'normal';
                $scope.Designer.FontID = $scope.getActiveProp('fontFamily');
            }

            $timeout(function () {
                if ($scope.Designer.type == 'Field')
                    $scope.Designer.FieldID = $scope.getActiveProp('text');
                if ($scope.Designer.type == 'QR' || $scope.Designer.type == 'Barcode')
                    $scope.Designer.FieldID = $scope.getActiveProp('field');

                $scope.Designer.showfont = ($scope.Designer.type == 'Text' || $scope.Designer.type == 'Field');
                $scope.Designer.showfield = ($scope.Designer.type == 'QR' || $scope.Designer.type == 'Barcode' || $scope.Designer.type == 'Field');
                $scope.Designer.showupload = ($scope.Designer.type == 'Image');
                $scope.Designer.showfill = ($scope.Designer.type == 'Rectangle' || $scope.Designer.type == 'Line' || $scope.Designer.type == 'Circle');
                $scope.$digest();
            }, 10);
        }

        $scope.RemoveActiveControl = function () {
            var activeObjects = $scope.canvas.getActiveObjects();
            $scope.canvas.discardActiveObject()
            if (activeObjects.length) {
                $scope.canvas.remove.apply($scope.canvas, activeObjects);
                $scope.SetActiveControl();
            }
        }

        $scope.processKeys = function (evt) {
            evt = evt || window.event;

            var movementDelta = 2;

            var activeObject = $scope.canvas.getActiveObject();

            if (evt.keyCode === 37) {
                evt.preventDefault(); // Prevent the default action
                if (activeObject) {
                    var a = activeObject.get('left') - movementDelta;
                    activeObject.set('left', a);
                }

            } else if (evt.keyCode === 39) {
                evt.preventDefault(); // Prevent the default action
                if (activeObject) {
                    var a = activeObject.get('left') + movementDelta;
                    activeObject.set('left', a);
                }

            } else if (evt.keyCode === 38) {
                evt.preventDefault(); // Prevent the default action
                if (activeObject) {
                    var a = activeObject.get('top') - movementDelta;
                    activeObject.set('top', a);
                }

            } else if (evt.keyCode === 40) {
                evt.preventDefault(); // Prevent the default action
                if (activeObject) {
                    var a = activeObject.get('top') + movementDelta;
                    activeObject.set('top', a);
                }
            } else if (evt.keyCode === 46) {
                evt.preventDefault(); // Prevent the default action
                if (activeObject) {
                    $scope.RemoveActiveControl();
                }
            }
            else if (evt.keyCode === 33) {
                evt.preventDefault(); // Prevent the default action
                if (activeObject) {
                    $scope.bringToFront();
                }
            } else if (evt.keyCode === 34) {
                evt.preventDefault(); // Prevent the default action
                if (activeObject) {
                    $scope.sendToBack();
                }
            }

            if (activeObject) {
                activeObject.setCoords();
                $scope.canvas.renderAll();
            }
        };

        $scope.canvas = null;
        $scope.InitializeIntractJS = function () {

            if ($scope.canvas == null) {

                var canvasWrapper = document.getElementById('canvas-wrapper');
                canvasWrapper.tabIndex = 1000;
                canvasWrapper.addEventListener("keydown", $scope.processKeys, false);
                canvasWrapper.addEventListener("click", function (evt) {
                    evt = evt || window.event;
                    if (evt.srcElement.id == 'canvas-wrapper') {
                        $scope.canvas.discardActiveObject();
                        $scope.canvas.renderAll();
                    }
                }, false);


                canvasWrapper.style.outline = "none";
                var canvas = new fabric.Canvas('barcodedesigner');

                canvas.on('object:scaling', function (e) {
                    var shape = e.target;
                    let sX = shape.scaleX;
                    let sY = shape.scaleY;
                    shape.set('scaleX', 1);
                    shape.set('scaleY', 1);
                    shape.set('width', sX * shape.width);
                    shape.set('height', sY * shape.height);
                });

                fabric.Object.prototype.transparentCorners = false;
                canvas.scope = $scope;
                $scope.canvas = canvas;
            }

            try {
                var canvas = $scope.canvas;
                var jsonObject = JSON.parse($scope.Configuration.reportformat.format);
                var objectList = [];
                $.each(jsonObject.objects, function (k, v) {
                    v.controlid = k + 1;
                    objectList[v.controlid] = v;
                });
                canvas.loadFromJSON(jsonObject, function () {
                    $scope.Designer.id = 0;
                    canvas.width = jsonObject.width;
                    canvas.height = jsonObject.height;
                    $.each(canvas.getObjects(), function (k, object) {
                        if ($scope.Designer.id < object.controlid)
                            $scope.Designer.id = object.controlid;

                        object.toObject = (function (toObject) {
                            return function () {
                                return fabric.util.object.extend(toObject.call(this), {
                                    controltype: this.controltype,
                                    controlid: this.controlid,
                                    field: this.field
                                });
                            };
                        })(object.toObject);

                        if (objectList[object.controlid]) {
                            object.width = objectList[object.controlid].width;
                            object.height = objectList[object.controlid].height;
                        }
                    });
                    canvas.setHeight(canvas.height);
                    canvas.setWidth(canvas.width);
                    canvas.renderAll();
                    $scope.CanvasHeight = canvas.height;
                    $scope.CanvasWidth = canvas.width;
                });

            } catch (e) {
                $scope.CanvasHeight = $scope.Configuration.reportformat.heightinmm;
                $scope.CanvasWidth = $scope.Configuration.reportformat.widthinmm;
            }

            //var rect = new fabric.Rect({
            //    left: 100,
            //    top: 50,
            //    width: 100,
            //    height: 100,
            //    fill: 'green',
            //    angle: 0,
            //    padding: 10
            //});
            //canvas.add(rect);

            //rect.source = rect;

            /*rect.on('selected', function (e) {
                alert('selected :'+JSON.stringify(e));
            });
            rect.on('deselected', function (e) {
                alert('deselected :' +JSON.stringify(e));
            });
            rect.on('modified', function (e) {
                alert('modified :' +JSON.stringify(e));
            });*/

            //fabric.Image.fromURL('clientside/dist/img/man.png', function (image) {
            //    image.set({
            //        left: 175,
            //        top: 175
            //    })
            //    .scale(1)
            //    .setCoords();
            //    canvas.add(image);
            //});

            //var barcode = new fabric.Text('test', {
            //    left: 150,
            //    top: 50,
            //    fontFamily: 'myFirstFont'
            //});
            //canvas.add(barcode);

            //canvas.on('mouse:down', function (e) {
            //    if (e.target) {
            //        //alert('mouse:down :' + JSON.stringify(e));
            //    }
            //});

            //var circle = new fabric.Circle({ radius: 30, fill: '#f55', top: 100, left: 100 });
            //canvas.add(circle);

            //var text = new fabric.Text('hello world', { left: 100, top: 100 });
            //canvas.add(text);

            //var textbox = new fabric.Textbox('hello world', { left: 100, top: 00 });
            //canvas.add(textbox);

            //canvas.on('after:render', function () {
            //    //canvas.contextContainer.strokeStyle = '#555';

            //    //canvas.forEachObject(function (obj) {
            //    //    var bound = obj.getBoundingRect();

            //    //    canvas.contextContainer.strokeRect(
            //    //        bound.left + 0.5,
            //    //        bound.top + 0.5,
            //    //        bound.width,
            //    //        bound.height
            //    //    );
            //    //})
            //});

            canvas.on('mouse:down', function (e) {
                if (e.target) {
                    if ($scope.Designer.currentid != e.target.controlid)
                        $scope.SetActiveControl(e.target);
                }
                else {
                    $scope.SetActiveControl(null);
                }
            });
        }

        $scope.CheckMM = function (value, source) {
            if (typeof (value) == 'undefined' || value.toString().length == 0 || !value.toString().endsWith('"')) return;
            if (source == 'height') {
                $scope.Configuration.reportformat.heightinmm = parseFloat(value.toString().replace('"', '')) * 25.4;
            }

            if (source == 'width') {
                $scope.Configuration.reportformat.widthinmm = parseFloat(value.toString().replace('"', '')) * 25.4;
            }

            $scope.SetCanvasSize($scope.Configuration.reportformat.heightinmm,
                $scope.Configuration.reportformat.widthinmm);
        }

        $scope.CloseLabelDesigner = function () {
            $scope.ShowDesigner = false;
        }

        $scope.ShowLabelDesigner = function () {
            $scope.ShowDesigner = true;
            $timeout(function () {
                $scope.InitializeIntractJS();
                $scope.$$phase || $scope.$digest();
            }, 10);
        }


        $scope.Sent2Printer = function (printdocument, printerData) {
            $rootScope.Sent2Printer(printdocument.join(';'), printerData);
            return;

            $.each(printdocument, function (k, v) {

                $rootScope.Sent2Printer(printdocument, printerData);

                //var request = {
                //    'id': printerData.printerid,
                //    'content': v,
                //    'encoded': 'BASE64'
                //};
                //var srequest = JSON.stringify(request);
                //$http({
                //    method: 'POST',
                //    url: printerData.service,
                //    data: srequest
                //}).success(function (result) {

                //}).error(function (error) {

                //});
            })
        }

        $scope.ValidteXML = function (sMyString) {
            var oParser = new DOMParser();
            var oDOM = oParser.parseFromString(sMyString, "text/xml");
            return oDOM.getElementsByTagName('parsererror').length ?
                oDOM.getElementsByTagName('parsererror')[0].innerHTML : "";
        }

        $scope.Save = function () {
            if ($scope.Configuration.key == null || $scope.Configuration.key == "") {
                $rootScope.Toaster.Info("Please Select Report Type");
                return;
            }

            if ($scope.Configuration.name == null || $scope.Configuration.name == "") {
                $rootScope.Toaster.Info("Please Enter Name of the Report");
                return;
            }

            //if ($scope.Configuration.organizationid == null || $scope.Configuration.organizationid == 0) {
            //    $rootScope.Toaster.Info("Please Select Company and Location");
            //    return;
            //}

            if ($scope.Configuration.key != 'BARCODE'
                && $scope.Configuration.key != 'CHEQUEPRINT'
                && $scope.Configuration.key != 'COUPONPRINT'
                && $scope.Configuration.key != 'GIFTBARCODE'
                && $scope.Configuration.key != 'EMPLOYEECODE'
                && $scope.Configuration.key != 'LRBUNDLELABEL'
                && !($scope.SupportEstamteSticker && $scope.Configuration.key == 'ESTIMATEBILL')) {

                var result = $scope.ValidteXML($scope.Configuration.reportformat.format);
                if (result != '') {
                    $rootScope.Toaster.Error(result);
                    return;
                }
            }

            request = {
                "Item": $scope.Configuration,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ConfigurationService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $scope.Configuration.id = result.id;
                    $rootScope.Toaster.Info("Successfully saved");
                    $scope.SearchHistory(result);
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
                $rootScope.Toaster.Error(result.ErrorMessage);
            });
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete Report ' + item.name + " ?",
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
                                $http({
                                    method: 'POST',
                                    url: "ConfigurationService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                    }

                                }).error(function (result) {

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
            else if (action == "Edit") {
                $scope.Navigation.SetMode('EDIT');
                $scope.ShowSearch = false;
                $scope.Configuration = item;
                $scope.ReportChanged();
                $scope.SearchHistory(item);
            }
        }
        $scope.HistoryList = [];
        $scope.SearchHistory = function (item) {
            request = {
                "Item": { name: item.name, organizationid: item.organizationid, locationid: item.locationid, history: true },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ConfigurationService/Search",
                data: srequest
            }).success(function (result) {

                $scope.HistoryList = result;
            }).error(function (result) {
            });

        }

        $scope.LoadHistory = function (item) {
            $scope.uploadfile = null;
            $scope.Configuration.reportformat = item.reportformat;
        }


        $scope.Import = function (data) {
            var item = (JSON.parse(data));
            $scope.Configuration.reportformat = item.reportformat;
            $scope.Configuration.nooflines = item.nooflines;
            $scope.Configuration.noofcopy = item.noofcopy;
            $scope.Configuration.instancename = item.instancename;
            $scope.$apply();
        }

        $scope.Export = function () {
            var blob = new Blob([JSON.stringify($scope.Configuration)],
                { type: "text/plain;charset=utf-8" });
            saveAs(blob, "Report.Config");
        }

        $scope.ApplicationSettings = function () {
            if (typeof ($rootScope.UserInfo) != 'undefined' && $rootScope.UserInfo != null &&
                typeof ($rootScope.UserInfo.appsettings) != 'undefined' && $rootScope.UserInfo.appsettings != null) {
                $scope.SupportEstamteSticker = false || $rootScope.UserInfo.appsettings.system_supportestimatesticker;
                $scope.ShowPaperSize = false || $rootScope.UserInfo.appsettings.system_supportpapersize;
            }
            else
                $timeout($scope.ApplicationSettings(), 100);
        }

        $scope.ptuploadedcomplete = function (result) {
            $scope.Configuration.reportformat.format = result;
        }

        $timeout(function () {
            $scope.ApplicationSettings();
            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 120) + 'px';
                $scope.canvasheight.height = '' + (available - 70) + 'px';
            }
        }, 100);
    }]);