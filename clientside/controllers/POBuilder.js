
var app = angular.module('InfinityLab');

app.controller('POBuilderController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', '$stateParams', 'cntxService', '$state',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $stateParams, cntxService, $state) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;
        $scope.panelheight = {'height':'500px'};
        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
            $scope.LocalContext = cntxService.CreatePageContext('POBuilder');
            //$stateParams.list = $scope.LocalContext.Get('ItemList', false);
            //if ($stateParams.list && $stateParams.list != null) {
            //    $stateParams.directlaunch = false;
            //    $stateParams.source = 'Purchase Plan';
            //}
            if (!$stateParams.directlaunch) {
                $timeout(function () {
                    if ($stateParams.source == 'Purchase Plan')
                        $scope.LoadFromPurchasePlan($stateParams.list);
                }, 50);
            }
        });

        $scope.Colors = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Colors = results;
        });

        $scope.Patterns = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Patterns = results;
        });

        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
        });

        $scope.Materials = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Materials = results;
        });

        $scope.Styles = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Styles = results;
        });

        $scope.TypeList = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.TypeList = results;
        });

        $scope.Brands = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brands = results;
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });


        $scope.RowEntity = {
            "productid": 0,
            "brandid": 0,
            "sizeid": 0,
            "patternid": 0,
            "styleid": 0,
            "colourid": 0,
            "typeid": 0,
            "materialid": 0,
            "order": 0,
            "required": 0,
            "key": ''
        };

        $scope.Entity = {
            "patterns": { items: [] },
            "sizes": { items: [] },
            "styles": { items: [] },
            "materials": { items: [] },
            "colours": { items: [] },
            "types": { items: [] },
            "brands": { items: [] },
            "products": { items: [] },
            "supplierid": 0
        };

        $scope.POBuilder = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.POBuilder = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";



        $scope.SearchCondition = {
            code: "",
            name: ""
        }

        $scope.New = function () {
            $scope.POBuilder = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }

        $scope.Save = function () {

            if ($scope.POBuilder.code == null || $scope.POBuilder.code == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please select Code");
                return;
            }

            if ($scope.POBuilder.name == null || $scope.POBuilder.name == 0) {
                $scope['setFocusname']();
                $rootScope.Toaster.Error("Please select Name");
                return;
            }

            if ($scope.POBuilder.printingname == null || $scope.POBuilder.printingname == 0) {
                $scope['setFocusprintingname']();
                $rootScope.Toaster.Error("Please select Printing Name");
                return;
            }


        }

  
        $scope.igrid = null;
        $scope.ManageList = function (action, value, name, list) {
            if (action == 'deleteall') {
                $scope.POBuilder.patterns = { items: [] };
                $scope.POBuilder.sizes = { items: [] };
                $scope.POBuilder.styles = { items: [] };
                $scope.POBuilder.materials = { items: [] };
                $scope.POBuilder.colours = { items: [] };
                $scope.POBuilder.types = { items: [] };
                $scope.POBuilder.brands = { items: [] };
                $scope.POBuilder.products = { items: [] };
                $scope.SearchResult = [];
                if ($scope.igrid !=null)
                    $scope.igrid.UpdateData($scope.SearchResult);
                return;
            }

            if (action == 'add') {
                var available = list.find(function (item) {
                    return item.id == value;
                })
                if (typeof (available) == 'undefined' || available == null)
                    list.push({ 'id': value, 'ro': 0, 'name': name });
            }
            else if (action == 'delete') {
                list.splice(list.indexOf(value), 1);
            }
            else if (action == 'jump') {


                var sizeidList = value.toString().split(',');
                var sizeList = sizeidList[0].split('-');
                if (sizeList.length == 2 && /^[a-zA-Z2-6]*$/.test(sizeList[0])) {

                    var sizeIndex = '3XS,XXS,XS,S,M,L,XL,XXL,XXXL,4XL,5XL,6XL,7XL'.split(',');
                    var startsize = sizeIndex.indexOf(sizeList[0]);
                    var endsize = sizeIndex.indexOf(sizeList[1]);
                    if (startsize < 0 || endsize < 0) {
                        sizeIndex = '3XS,XXS,XS,S,M,L,XL,2XL,3XL,4XL,5XL,6XL,7XL'.split(',');
                        startsize = sizeIndex.indexOf(sizelist[0]);
                        endsize = sizeIndex.indexOf(sizelist[1]);
                    }
                    sizeList = [];
                    for (var index = startsize; index <= endsize; index++) {
                        sizeList.push(sizeIndex[index]);
                    }

                }
                else if (sizeList.length == 3) {

                    var firstPart = sizeList[0].match(/[^-_ \d]+|\d+/g);
                    var lastPart = sizeList[2].match(/[^-_ \d]+|\d+/g);
                    var suffix = '';
                    var startsize = parseInt(sizeList[0]);
                    var increment = parseInt(sizeList[1]);
                    var endsize = parseInt(sizeList[2]);

                    if (firstPart.length == 2 && /^[a-zA-Z2-6]*$/.test(firstPart[1]))
                        suffix = firstPart[1];

                    sizeList = [];
                    for (; startsize <= endsize; startsize = startsize + increment) {
                        var searchsize = startsize.toString() + suffix;

                        var excluded = false;
                        for (var i = 1; i < sizeidList.length; i++) {
                            if (sizeidList[i] == searchsize)
                                excluded = true;
                        }
                        if (!excluded)
                            sizeList.push(searchsize);
                    }
                }

                var idList = [];
                $.each($scope.Sizes, function (k, v) {
                    $.each(sizeList, function (kk, vv) {
                        if (vv == v.name) {

                            if (idList.find(function (item) {
                                return item.name == v.name;
                            }) == null) {

                                var available = list.find(function (item) {
                                    return item.id == v.id;
                                })
                                if (typeof (available) == 'undefined' || available == null)
                                    list.push({ 'id': v.id, 'name': v.name });
                            }
                        }
                    });
                });
            }
            $scope.BuildRows();
        }



        $scope.Columns = [];
        $scope.SearchResult = [];

        $scope.BuildInnerRows = function (rows, values, fieldid) {
            var newrows = [];
            if (rows.length == 0) {
                $.each(values.items, function (k, v) {
                    var row = JSON.parse(JSON.stringify($scope.RowEntity));
                    row[fieldid] = v.id;
                    newrows.push(row);
                });
            }
            else {
                $.each(rows, function (k, v) {
                    $.each(values.items, function (kk, vv) {
                        var row = JSON.parse(JSON.stringify(v));
                        row[fieldid] = vv.id;
                        newrows.push(row);
                    });
                });
            }

            if (newrows.length == 0) return rows;
            rows = null;
            return newrows;
        }

        $scope.BuildRows = function () {
            var totalrows = 0;
            var rows = [];
            //if ($scope.POBuilder.products.items && $scope.POBuilder.products.items.length > 0)
            //    totalrows += $scope.POBuilder.products.items.length;
            //if ($scope.POBuilder.brands.items && $scope.POBuilder.brands.items.length > 0)
            //    totalrows *= $scope.POBuilder.brands.items.length;
            //if ($scope.POBuilder.sizes.items && $scope.POBuilder.sizes.items.length > 0)
            //    totalrows *= $scope.POBuilder.sizes.items.length;
            //if ($scope.POBuilder.styles.items && $scope.POBuilder.styles.items.length > 0)
            //    totalrows *= $scope.POBuilder.styles.items.length;
            //if ($scope.POBuilder.patterns.items && $scope.POBuilder.patterns.items.length > 0)
            //    totalrows *= $scope.POBuilder.patterns.items.length;
            //if ($scope.POBuilder.types.items && $scope.POBuilder.types.items.length > 0)
            //    totalrows *= $scope.POBuilder.types.items.length;
            //if ($scope.POBuilder.materials.items && $scope.POBuilder.materials.items.length > 0)
            //    totalrows *= $scope.POBuilder.materials.items.length;
            //if ($scope.POBuilder.colours.items && $scope.POBuilder.colours.items.length > 0)
            //    totalrows *= $scope.POBuilder.colours.items.length;
            //for (var i = 0; i < totalrows; i++)
            //    rows.push(JSON.parse(JSON.stringify($scope.RowEntity)));

            rows = $scope.BuildInnerRows(rows, $scope.POBuilder.products, 'productid');
            rows = $scope.BuildInnerRows(rows, $scope.POBuilder.brands, 'brandid');
            rows = $scope.BuildInnerRows(rows, $scope.POBuilder.styles, 'styleid');
            rows = $scope.BuildInnerRows(rows, $scope.POBuilder.patterns, 'patternid');
            rows = $scope.BuildInnerRows(rows, $scope.POBuilder.sizes, 'sizeid');
            rows = $scope.BuildInnerRows(rows, $scope.POBuilder.colours, 'colourid');
            rows = $scope.BuildInnerRows(rows, $scope.POBuilder.materials, 'materialid');
            rows = $scope.BuildInnerRows(rows, $scope.POBuilder.types, 'typeid');

            var hasbrand = $scope.Columns.indexOf('Brands') == -1;
            var hasstyle = $scope.Columns.indexOf('Style') == -1;
            var haspattern = $scope.Columns.indexOf('Pattern') == -1;
            var hassize = $scope.Columns.indexOf('Size') == -1;
            var hascolour = $scope.Columns.indexOf('Colour') == -1;
            var hasmaterial = $scope.Columns.indexOf('Material') == -1;
            var hastype = $scope.Columns.indexOf('Type') == -1;

            $.each($scope.SearchResult, function (k, v) {
                var row = rows.find(function (item) {
                    return item.productid == v.productid &&
                        (item.brandid == v.brandid || hasbrand ) &&
                        (item.styleid == v.styleid || hasstyle  ) &&
                        (item.patternid == v.patternid || haspattern  ) &&
                        (item.sizeid == v.sizeid || hassize  ) &&
                        (item.colourid == v.colourid || hascolour  ) &&
                        (item.materialid == v.materialid || hasmaterial  ) &&
                        (item.typeid == v.typeid || hastype );
                });
                if (row && row.productid == v.productid) {
                    row.order = v.order;
                    row.required = v.required;
                }
            });

            $scope.SearchResult = rows;
            if ($scope.POBuilderConfig()) {
                $scope.igrid.UpdateData($scope.SearchResult);
            }
        }

        $scope.POBuilderConfig = function () {

            var reconfig = false;
            if ($scope.Columns.indexOf('Products') == -1 &&
                $scope.POBuilder.products.items && $scope.POBuilder.products.items.length > 0) {
                $scope.Columns.push('Products');
                reconfig = true;
            }
            if ($scope.Columns.indexOf('Brands') == -1 &&
                $scope.POBuilder.brands.items && $scope.POBuilder.brands.items.length > 0) {
                $scope.Columns.push('Brands');
                reconfig = true;
            }
            if ($scope.Columns.indexOf('Size') == -1 &&
                $scope.POBuilder.sizes.items && $scope.POBuilder.sizes.items.length > 0) {
                $scope.Columns.push('Size');
                reconfig = true;
            }
            if ($scope.Columns.indexOf('Style') == -1 &&
                $scope.POBuilder.styles.items && $scope.POBuilder.styles.items.length > 0) {
                $scope.Columns.push('Style');
                reconfig = true;
            }
            if ($scope.Columns.indexOf('Pattern') == -1 &&
                $scope.POBuilder.patterns.items && $scope.POBuilder.patterns.items.length > 0) {
                $scope.Columns.push('Pattern');
                reconfig = true;
            }
            if ($scope.Columns.indexOf('Colour') == -1 &&
                $scope.POBuilder.colours.items && $scope.POBuilder.colours.items.length > 0) {
                $scope.Columns.push('Colour');
                reconfig = true;
            }
            if ($scope.Columns.indexOf('Type') == -1 &&
                $scope.POBuilder.types.items && $scope.POBuilder.types.items.length > 0) {
                $scope.Columns.push('Type');
                reconfig = true;
            }
            if ($scope.Columns.indexOf('Material') == -1 &&
                $scope.POBuilder.materials.items && $scope.POBuilder.materials.items.length > 0) {
                $scope.Columns.push('Material');
                reconfig = true;
            }

            if (!reconfig) return true;

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            if ($scope.Columns.indexOf('Products') >= 0)
                igrid.AddReferenceColumn('productid', 'Product', 100, $scope.Products);
            if ($scope.Columns.indexOf('Brands') >= 0)
                igrid.AddReferenceColumn('brandid', 'Brand', 100, $scope.Brands);
            if ($scope.Columns.indexOf('Size') >= 0)
                igrid.AddReferenceColumn('sizeid', 'Size', 100, $scope.Sizes);
            if ($scope.Columns.indexOf('Style') >= 0)
                igrid.AddReferenceColumn('styleid', 'Style', 100, $scope.Styles);
            if ($scope.Columns.indexOf('Pattern') >= 0)
                igrid.AddReferenceColumn('patternid', 'Pattern', 100, $scope.Patterns);
            if ($scope.Columns.indexOf('Colour') >= 0)
                igrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.Colors);
            if ($scope.Columns.indexOf('Type') >= 0)
                igrid.AddReferenceColumn('typeid', 'Type', 100, $scope.TypeList);
            if ($scope.Columns.indexOf('Material') >= 0)
                igrid.AddReferenceColumn('materialid', 'Material', 100, $scope.Materials);

            igrid.AddColumn(
                {
                    id: "Required", name: "Required", field: "required",
                    width: 60, cssClass: "cell-title" , 'sortable':true
                },
            );
            igrid.AddColumn(
                {
                    id: "Order", name: "Order", field: "order",
                    width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                },
            );



            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
            return false;
        }

        $scope.AddList = function (list, value, display) {
            var available = list.find(function (item) {
                return item.id == value;
            })
            if (typeof (available) == 'undefined' || available == null)
                list.push({ 'id': value, 'ro': 0, 'name': display });
        }

        $scope.Convert2PO = function () {
            var rows = [];

            $.each($scope.SearchResult, function (k, v) {
                if (v.order && v.order > 0)
                    rows.push(v);
            });

            $scope.LocalContext = cntxService.CreatePageContext('POBuilder');
            $scope.LocalContext.Post('OrderList', rows, false);
            $state.go('PurchaseOrder', {
                directlaunch: false,
                source: 'PO Builder',
                list: rows
            });

        }

        $scope.LoadFromPurchasePlan = function (list) {

            $scope.SearchResult = [];
            $.each(list, function (k, v) {
                var orderqty = v.cqty - v.sqty;
                if (orderqty > 0) {
                    var row = {
                        'order': orderqty, 'required': orderqty
                    };
                    $scope.SearchResult.push(row);
                    $.each(v.columns, function (kk, vv) {

                        switch (vv.type) {
                            case 'Product':
                                row.productid = vv.value;
                                $scope.AddList($scope.POBuilder.products.items, vv.value, vv.display);
                                break;
                            case 'Style':
                                row.styleid = vv.value;
                                $scope.AddList($scope.POBuilder.styles.items, vv.value, vv.display);
                                break;
                            case 'Colour':
                                row.colourid = vv.value;
                                $scope.AddList($scope.POBuilder.colours.items, vv.value, vv.display);
                                break;
                            case 'Size':
                                $scope.AddList($scope.POBuilder.sizes.items, vv.value, vv.display);
                                row.sizeid = vv.value;
                                break;
                            case 'Brand':
                                $scope.AddList($scope.POBuilder.brands.items, vv.value, vv.display);
                                row.brandid = vv.value;
                                break;
                            case 'Type':
                                $scope.AddList($scope.POBuilder.types.items, vv.value, vv.display);
                                row.typeid = vv.value;
                                break;
                            case 'Material':
                                $scope.AddList($scope.POBuilder.materials.items, vv.value, vv.display);
                                row.materialid = vv.value;
                                break;
                            case 'Pattern':
                                $scope.AddList($scope.POBuilder.patterns.items, vv.value, vv.display);
                                row.patternid = vv.value;
                                break;
                        }
                    });
                }
            });

            $scope.POBuilderConfig();
            $scope.BuildRows();
        }

        $timeout(function () {
            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 75) + 'px';
            }
            $scope.setFocusfirst();
        }, 100);
    }]);



