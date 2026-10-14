const app = angular.module('catalogApp', ['angular-inview']);


app.controller('CatalogController', function ($scope, $window, $http, $timeout) {
    var ngAuthSettings = {
        clientId: 'InfyPOS',
        secret: 'KareInfinity',
        org: ''
    };

    $scope.isDesktopDevice = function() {
        const isTouchDevice = 'ontouchstart' in window || navigator.maxTouchPoints > 0;
        const userAgent = navigator.userAgent.toLowerCase();
        const isMobileOrTablet = /android|iphone|ipad|ipod|mobile|tablet/i.test(userAgent);

        // If it's touch-enabled and the user agent says mobile/tablet, it's not desktop
        if (isTouchDevice && isMobileOrTablet) {
            return false;
        }

        // Fallback to screen size if needed
        return window.innerWidth >= 1024; // more realistic than 768
    }

    $scope.vm = {
        showFilters: false,
        perRow: 1,
        isDesktop: $scope.isDesktopDevice(),
        filters: {
            keyword: '',
            brand: '',
            size: '',
            status: 'Any',
            statusimg:true
        },
        sizes: ['S', 'M', 'L', 'XL'],
        brands: ['Brand A', 'Brand B', 'Brand C'],
        designs: [
        ],
        loginData: {
            username: '',
            password: '',
            orgName: '',
            useRefreshTokens: true,
            deviceid: 'web-client-' + Date.now()
        },
        designFilterFn: function (item) {
            if (!$scope.vm.filterdesign || $scope.vm.filterdesign.trim() === '') return true;
            return item.designitemcode
                .toLowerCase()
                .includes($scope.vm.filterdesign.toLowerCase());
        },
        designGroupFilterFn: function (item) {
            if (!$scope.vm.filterdesign || $scope.vm.filterdesign.trim() === '') return true;
            return item.currentdesign.designitemcode
                .toLowerCase()
                .includes($scope.vm.filterdesign.toLowerCase());
        },
        refreshToken: function () {
            const data = "grant_type=refresh_token&refresh_token=" + $scope.vm.authData.refreshToken + "&client_id=" + ngAuthSettings.clientId +
                "&client_secret=" + ngAuthSettings.secret + "&org=" + encodeURIComponent(ngAuthSettings.org);
            return $http({
                method: 'POST',
                url: $scope.vm.apiBase + "token",
                data: data,
                headers: { 'Authorization': 'Bearer ' + $scope.vm.authData.token, 'Content-Type': 'application/x-www-form-urlencoded' }
            }).then(function (response) {
                $scope.vm.saveAuthData(response.data);
            }).catch(function () {
                $scope.vm.logout();
            });
        },
        authenticate: function () {
            var loginData = $scope.vm.loginData;
            $scope.vm.showauthFailed = false;


            var data = "grant_type=password&username=" + encodeURIComponent(loginData.username) +
                "&password=" + encodeURIComponent(loginData.password) +
                "&org=" + encodeURIComponent(loginData.orgName) +
                "&client_id=" + ngAuthSettings.clientId +
                "&client_secret=" + ngAuthSettings.secret +
                '&platform=WEB&deviceid=' + encodeURIComponent(loginData.deviceid) +
                '&TZ=' + new Date().getTimezoneOffset();

            $http.post('token', data, {
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
            }).then(function (response) {
                var res = response.data;

                var authData = {
                    token: res.access_token,
                    username: res.username,
                    refreshToken: loginData.useRefreshTokens ? res.refresh_token : "",
                    refreshTokenHash: loginData.useRefreshTokens ? res.refresh_token_hash : "",
                    useRefreshTokens: loginData.useRefreshTokens,
                    FullData: res
                };

                localStorage.setItem('ls.authorizationData', JSON.stringify(authData));
                window.location.href = 'catalog.html?TZ=' + new Date().getTime();
            }, function (err) {
                $scope.vm.usernameInput.focus();
                $scope.vm.showauthFailed_description = err.data.error_description;
                $scope.vm.showauthFailed = true;
            });
        }
    };


    $scope.vm.loadLoginDialog = function () {

        setTimeout(function () {

            $scope.vm.usernameInput = document.querySelector('input[ng-model="vm.loginData.username"]');
            $scope.vm.passwordInput = document.querySelector('input[ng-model="vm.loginData.password"]');

            if ($scope.vm.usernameInput && $scope.vm.passwordInput) {
                $scope.vm.usernameInput.focus();
                $scope.vm.usernameInput.addEventListener('keydown', function (e) {
                    if (e.key === 'Enter') {
                        $scope.vm.passwordInput.focus();
                    }
                });
                $scope.vm.passwordInput.addEventListener('keydown', function (e) {
                    if (e.key === 'Enter') {
                        $scope.vm.authenticate();
                    }
                });
            }
        }, 100);
    }

    $scope.vm.onSizeKeyDown = function (event) {
        // Allow only number keys, numpad numbers, backspace, tab
        const allowedKeys = [
            8,  // backspace
            9,  // tab
            48, 49, 50, 51, 52, 53, 54, 55, 56, 57,       // 0–9
            96, 97, 98, 99, 100, 101, 102, 103, 104, 105  // numpad 0–9
        ];

        if (!allowedKeys.includes(event.keyCode)) {
            return;
        }

        // Move focus only when a number is pressed
        if (
            (event.keyCode >= 48 && event.keyCode <= 57) ||
            (event.keyCode >= 96 && event.keyCode <= 105)
        ) {
            event.preventDefault();

            const inputs = document.querySelectorAll('.size-input');
            const currentIndex = Array.prototype.indexOf.call(inputs, event.target);

            if (currentIndex > -1 && currentIndex < inputs.length - 1) {
                inputs[currentIndex + 1].focus();
            }
        }
    };

    // Load auth info from localStorage
    $scope.vm.fillAuthData = function () {
        const authData = JSON.parse(localStorage.getItem('ls.authorizationData'));
        if (authData) {
            $scope.vm.authData = authData;
            return true;
        }
        return false;
    };

    // Clear auth info
    $scope.vm.logout = function () {
        $scope.vm.authData = { token: "", refreshToken: "", useRefreshTokens: true };
        localStorage.removeItem('ls.authorizationData');
        $scope.vm.viewMode = null;
        $scope.vm.authFailed = true;
        $scope.vm.showLoginDialog = true;
        $scope.vm.loadLoginDialog();
    };

    $scope.vm.apiGet = function (url, retry) {
        return $http({
            method: 'GET',
            url: $scope.vm.apiBase + url,
            headers: {
                'Authorization': 'Bearer ' + $scope.vm.authData.token
            }
        }).catch(function (error) {
            if (error.status === 401) {
                if (retry > 3) return;
                return $scope.vm.refreshToken().then(() => {

                    if (retry == undefined) retry = 1;
                    return $scope.vm.apiGet(url, retry + 1);
                });
            }
            return Promise.reject(error);
        });
    };

    $scope.vm.apiPost = function (status, url, postData, retry) {
        $scope.vm.showProgress = true;
        $scope.vm.progressMessage = status;

        return $http({
            method: 'POST',
            url: $scope.vm.apiBase + url,
            data: postData,
            headers: {
                'Authorization': 'Bearer ' + $scope.vm.authData.token,
                'Content-Type': 'application/json'
            }
        }).finally(function () {
            $scope.vm.showProgress = false;
            $scope.vm.progressMessage = '';
        }).catch(function (error) {
            if (error.status === 417) {
                $scope.vm.showToast(error.data, 'error', 1000)
            }
            else if (error.status === 401) {
                if (retry > 3) return;
                return $scope.vm.refreshToken().then(() => {
                    if (retry == undefined) retry = 1;
                    return $scope.vm.apiPost(status, url, postData, retry + 1);
                });
            }
            return Promise.reject(error);
        });
    };



    $scope.vm.customers = [
        { name: 'ABC Traders', address: 'Chennai', gstno: '33ABCDE1234F1Z5' },
        { name: 'XYZ Enterprises', address: 'Mumbai', gstno: '27XYZDE5678G2Z6' }
    ];
    $scope.vm.hideFilters = false;
    $scope.vm.selectedCustomer = { 'id': 0, 'name': '', 'cityid': 0, 'stateid': 0, 'gstno': '' };
    $scope.vm.sizeIdNameMap = {};   // id -> name
    $scope.vm.sizeNameIdMap = {};   // name -> id
    $scope.vm.showCustomerModal = false;
    $scope.vm.showCustomerSearchModal = false;
    $scope.vm.newCustomer = { name: '', address: '', gstno: '' };

    $scope.vm.openCustomerSearchModal = function (isshiptto) {
        $scope.vm.Customer = { name: '', id: 0, stateid: 0, cityid: 0, address: '', gstno: '' };
        $scope.vm.showCustomerSearchModal = true;
        $scope.vm.isshiptto = isshiptto;
    };

    $scope.vm.closeCustomerSearchModal = function () {
        $scope.vm.showCustomerSearchModal = false;
    };

    $scope.vm.selectCustomerFromList = function (selectedItem) {
        selectedItem.cityname = $scope.vm.cities.find(c => c.id === selectedItem.cityid)?.name || null;
        selectedItem.statename = $scope.vm.states.find(c => c.id === selectedItem.stateid)?.name || null;

        if ($scope.vm.isshiptto) {
            $scope.vm.shiptoCustomer = selectedItem;
        }
        else {
            $scope.vm.selectedCustomer = selectedItem;
            $scope.vm.showCustomerSearchModal = false;
            $scope.vm.agentid = selectedItem.agentid;
            $scope.vm.marginmode = selectedItem.marginmode;
            $scope.vm.marginprice = selectedItem.marginprice;
            $scope.vm.pricesource = selectedItem.psource;
            $scope.vm.hasshipto = selectedItem.type == 7;
            if (selectedItem.billdiscount > 0)
                $scope.vm.discountpercentage = selectedItem.billdiscount;
            localStorage.setItem('selectedCustomer', JSON.stringify(selectedItem));
        }
    };

    $scope.vm.openCustomerModal = function () {
        $scope.vm.newCustomer = { name: '', address: '', gstno: '' };
        $scope.vm.showCustomerModal = true;
    };

    $scope.vm.closeCustomerModal = function () {
        $scope.vm.showCustomerModal = false;
    };

    $scope.vm.closeApprovalModal = function () {
        $scope.vm.showApprovalModal = false;
    };

    $scope.vm.openApprovalModal = function (currentSO) {
        $scope.vm.showApprovalModal = true;
        $scope.vm.showApprovalSOID = currentSO.id;
    };

    $scope.vm.sortColumn = '';
    $scope.vm.sortReverse = false;

    $scope.vm.sortBy = function (column) {
        if ($scope.vm.sortColumn === column) {
            $scope.vm.sortReverse = !$scope.vm.sortReverse; // toggle asc/desc
        } else {
            $scope.vm.sortColumn = column;
            $scope.vm.sortReverse = false;
        }
    };

    $scope.vm.saveApproval = function () {
        if ($scope.vm.approvedemployee && $scope.vm.approvedemployee>0) {
            var request = {
                "Item": { 'ids': [$scope.vm.showApprovalSOID], 'pickedby': $scope.vm.approvedemployee },
                "SecurityToken": "String content"
            }
            $scope.vm.apiPost('Saving ...Approval', 'SalesorderService/SetSMApproveval', request).then(function (response) {
                $scope.vm.closeApprovalModal();
            });
        } else {
            alert('Please select employee.');
        }
    };

    $scope.vm.selectedCustomerObject = null;

    $scope.vm.selectCustomer = function () {
        const selected = $scope.vm.customers.find(c => c.name === $scope.vm.selectedCustomer);
        $scope.vm.selectedCustomerObject = selected || null;
    };

   
    $scope.vm.saveCustomer = function () {
        if ($scope.vm.newCustomer.name && $scope.vm.newCustomer.address) {
            const newCust = angular.copy($scope.vm.newCustomer);
            newCust.newCust = -999;
            var request = {
                "Item": newCust,
                "SecurityToken": "String content"
            }
            $scope.vm.apiPost('Saving ...Customer', 'CustomerService/Save', request).then(function (response) {
                newCust.id = response.id;
                $scope.vm.customers.push(newCust);
                $scope.vm.selectedCustomer = newCust;
                $scope.vm.closeCustomerModal();
            });
        } else {
            alert('Please fill Name and address.');
        }
    };


    $scope.vm.apiBase = '';
    if ($scope.vm.fillAuthData()) {
        $scope.vm.apiGet('ReferencelistService/MYCUSTOMER').then(function (response) {
            $scope.vm.customers = response.data;
        });

        $scope.vm.apiGet('ReferencelistService/PRODUCTS').then(function (response) {
            response.data.push({ 'id': 0, 'name': '--ALL--' });
            $scope.vm.products = response.data;
        });

        $scope.vm.apiGet('ReferencelistService/MYLOCATION').then(function (response) {
            //response.data.push({ 'id': 0, 'name': '--ALL--' });
            $scope.vm.locations = response.data;
        });

        $scope.vm.apiGet('ReferencelistService/BRAND').then(function (response) {
            var brandwithimage = [];
            response.data.forEach(function (v) {
                if (v.data && v.data != null && v.data != '') {
                    brandwithimage.push({ 'name': v.name, 'id': v.id, 'logoid': v.data });
                }
            });
            response.data.push({ 'id': 0, 'name': '--ALL--' });
            $scope.vm.brands = response.data;
            $scope.vm.brandwithimage = brandwithimage;
        });

        $scope.vm.apiGet('ReferencelistService/STYLE').then(function (response) {
            response.data.push({ 'id': 0, 'name': '--ALL--' });
            $scope.vm.styles = response.data;
        });


        $scope.vm.apiGet('ReferencelistService/PATTERN').then(function (response) {
            response.data.push({ 'id': 0, 'name': '--ALL--' });
            $scope.vm.patterns = response.data;
        });

        $scope.vm.apiGet('ReferencelistService/TRANSPORT').then(function (response) {
            $scope.vm.transports = response.data;
        });

        $scope.vm.apiGet('ReferencelistService/STATE').then(function (response) {
            $scope.vm.states = response.data;
        });

        $scope.vm.apiGet('ReferencelistService/CITY').then(function (response) {
            $scope.vm.cities = response.data;
        });

        $scope.vm.apiGet('ReferencelistService/CLASSIFICATION').then(function (response) {
            response.data.push({ 'id': 0, 'name': '--ALL--' });
            $scope.vm.classes = response.data;
        });

        $scope.vm.apiGet('ReferencelistService/FULLCOMPANY').then(function (response) {
            $scope.vm.company = response.data;
        });

        $scope.vm.apiGet('ReferencelistService/AGENT').then(function (response) {
            $scope.vm.agents = response.data;
        });

        $scope.vm.apiGet('ReferencelistService/EMPLOYONLY').then(function (response) {
            $scope.vm.employees = response.data;
        });

        $scope.vm.apiGet('ReferencelistService/ADDITIONALCHARGES', true).then(function (response) {
            $scope.vm.BoxList = [];
            $scope.vm.BoxList.push({ 'id': -1, 'name': 'Designwise', 'add': 0, 'remove': 0 });
            response.data.forEach(function (v) {
                if (v.additionalinfo && v.additionalinfo.paytype == 'BOX') {
                    $scope.vm.BoxList.push({ 'id': v.id, 'name': v.name, 'add': v.additionalinfo.maxlimit, 'remove': v.additionalinfo.billdiscount });
                }
            });
            $scope.vm.SupportBoxCharges = $scope.vm.BoxList.length > 1;
        });

        $scope.vm.apiGet('ReferencelistService/SIZE').then(function (response) {
            $scope.vm.fullsizes = response.data;

            response.data.forEach(function (size) {
                $scope.vm.sizeIdNameMap[size.id] = size.name;
                $scope.vm.sizeNameIdMap[size.name] = size.id;
            });

            if ($scope.vm.sizes == undefined || $scope.vm.sizes == null)
                $scope.vm.sizes = response.data.map(size => size.name);

            $scope.vm.apiGet('ReferencelistService/FULL-SIZEGROUP').then(function (response) {
                response.data.push({ 'id': 0, 'name': '--ALL--' });
                $scope.vm.fullsizegroup = response.data;
                response.data.forEach(function (v) {
                    if (v.additionalinfo && v.additionalinfo.sizegroups && v.additionalinfo.sizegroups.length > 0) {

                        v.possizelist = [];

                        v.additionalinfo.sizegroups.forEach(function (vv) {
                            var size = $scope.vm.fullsizes.find(function (item) {
                                return item.id == vv;
                            });
                            if (size) {
                                v.possizelist.push(size.name);
                            }
                        });

                        //if (v.additionalinfo.billdiscountinclude) {
                        //    $scope.mfg_sosizeratioList.push({
                        //        id: v.id,
                        //        name: v.name,
                        //        possizelist: v.possizelist
                        //    });
                        //}

                        v.possizelist = v.possizelist.join(',');
                    }
                });
                $scope.vm.sizegroupchanged();
            });

        });
    }
    else {
        $scope.vm.authFailed = true;
        $scope.vm.showLoginDialog = true;
        $scope.vm.loadLoginDialog();
        return;
    }

    $scope.vm.StoreLayout = function () {
        localStorage.setItem('ls.layout',
            JSON.stringify({
                'showFilters': $scope.vm.showFilters, 'hideFilters': $scope.vm.hideFilters,
                'perRow': $scope.vm.perRow, 'brandid': $scope.vm.filters.brandid
            }));
    }

    $scope.vm.RestoreLayout = function () {
        var saved = localStorage.getItem('ls.layout');
        if (saved) {
            try {
                var layout = JSON.parse(saved);
                $scope.vm.showFilters = layout.showFilters;
                $scope.vm.hideFilters = layout.hideFilters;
                $scope.vm.perRow = layout.perRow;
                $scope.vm.filters.brandid = layout.brandid;
            } catch (e) {
                console.error('Failed to parse saved modified orders', e);
            }
        } 
    }

    $scope.vm.ConfirmWidow = {
        'enabled': false,
        'Message': function (e) {
            e.preventDefault();
            e.returnValue = '';
        },
        'SetEnable': function (value) {
            if (this.enabled == value) return;
            if (this.enabled = true && value == false)
                window.removeEventListener('beforeunload', this.Message);
            this.enabled = value;
            if (this.enabled)
                window.addEventListener('beforeunload', this.Message);
        }
    };


    $scope.vm.Search = function () {

        //if ($scope.vm.filters.sizegroupid == undefined ||
        //    $scope.vm.filters.sizegroupid == null) {
        //    $scope.vm.showToast('Please select Size Group', 'warning', 1);
        //    return;
        //}

        var designid = null;
        if ($scope.vm.filters.designid && $scope.vm.filters.designid != null && $scope.vm.filters.designid != '') {
            designid = $scope.vm.filters.designid.toUpperCase();
            if (!designid.endsWith('%')) {
                designid += '%';
            }
        }

        var searchCondition = {
            "item": {
                'productid': $scope.vm.filters.productid,
                'brandid': $scope.vm.filters.brandid,
                'styleid': $scope.vm.filters.styleid,
                'classid': $scope.vm.filters.classid,
                'patternid': $scope.vm.filters.patternid,
                'designid': designid,
                'sizegroupid': $scope.vm.filters.sizegroupid,
            },
            "securitytoken": "String content"
        };

        if ($scope.vm.filters.status && $scope.vm.filters.status != null) {
            switch ($scope.vm.filters.status) {
                case 'Stock':
                    searchCondition.item.onlystock = true;
                    break;
                case 'WIP':
                    searchCondition.item.onlywip = true;
                    break;
                case 'Both':
                    searchCondition.item.onlywip = true;
                    searchCondition.item.onlystock = true;
                    break;
                case 'Any':
                    searchCondition.item.onlywip = true;
                    searchCondition.item.onlystock = true;
                    searchCondition.item.onlyso = true;
                    break;
            }
        }


        if ($scope.vm.filters.rule && $scope.vm.filters.rule != null) {
            switch ($scope.vm.filters.rule) {
                case '1':
                    searchCondition.item.onlyimage = true;
                    break;
                case '2':
                    searchCondition.item.onlyassorted = true;
                    break;
                case '3':
                    searchCondition.item.assortedzero = true;
                    break;
            }
        }

        if ($scope.vm.filters.statusimg) {
            searchCondition.item.onlyimage = true;
        }

        $scope.vm.cleanModifiedOrders();
        $scope.vm.canRestore = false;
        $scope.vm.apiPost('Searching ...Designs', 'MfgReportService/SearchInGallery', searchCondition).then(function (response) {
            if (response == undefined || response == null ||
                response.data == undefined || response.data == null) {
                $scope.vm.showToast('Cant find any designs', 'warning', 1);
                    return;
                }
            $scope.vm.designsdata = response.data;
            $scope.vm.designs = $scope.vm.designsdata.items;
            $scope.vm.availDesigns = $scope.vm.designs.length;
            $scope.vm.groupedDesigns = [];
            $scope.vm.groupedDesignsCount = 0;
            $scope.vm.colourgroup = $scope.vm.designsdata.colourgroup;
            $scope.vm.sizes = $scope.vm.designsdata.sizelist.map(size => size.name);
            $scope.limit = 20
            $scope.vm.mergeModifiedOrders(); // bring back modified ones
            $scope.vm.recalculateSizeTotals(); // fix size totals

            if ($scope.vm.company && $scope.vm.company.length == 1)
                $scope.vm.companyid = $scope.vm.company[0].id;

            if ($scope.vm.locations && $scope.vm.locations.length == 1)
                $scope.vm.deliverylocationid = $scope.vm.locations[0].id;

            if ($scope.vm.customers && $scope.vm.customers.length == 1)
                vm.selectedCustomer = $scope.vm.customers[0];

            if ($scope.vm.perRow == -1) {
                $scope.vm.processGroupedDesign();
            }

        });
    }

    $scope.vm.groupMasterItemClicked = function (grpitem) {
        grpitem.currentdesign = grpitem.designs[0];
        grpitem.currentdesign.showmimgid = true;
    }

    $scope.vm.groupItemClicked = function (dsgnitem, grpitem) {
        if (dsgnitem == undefined || dsgnitem == null) return;
        if (grpitem.currentdesign.showmimgid)
            grpitem.currentdesign.showmimgid = false;
        grpitem.currentdesign = dsgnitem;
    }

    $scope.vm.processGroupedDesign = function () {
        if ($scope.vm.groupedDesigns.length > 0) return;
        $scope.gdlimit = 10;
        var groupedDesigns = [];
        angular.forEach($scope.vm.colourgroup, function (item) {
            var design = { 'currentdesign': null, 'designs': [], 'id': groupedDesigns.length + 1 };
            groupedDesigns.push(design);
            angular.forEach(item.designs, function (designitem) {
                var itemdesign = $scope.vm.designs.find(function (sitem) {
                    return sitem.designitemcode == designitem;
                });
                if (design.currentdesign == null) {
                    design.currentdesign = itemdesign;
                    if (itemdesign.mimgid && itemdesign.mimgid != '') {
                        design.currentdesign.showmimgid = true;
                        design.mimgid = itemdesign.mimgid;
                        design.hasmimgid = true;
                    }
                }
                if (item.designs.length > 1) {
                    design.designs.push(itemdesign);
                }
            });
        });
        $scope.vm.groupedDesigns = groupedDesigns;
        $scope.vm.groupedDesignsCount = groupedDesigns.length;
    }

    $scope.vm.availDesigns = $scope.vm.designs.length

    $scope.vm.SearchBill = function () {

        var addedfrom = '';
        var addedto = '';
        if ($scope.vm.filters.addedfrom && $scope.vm.filters.addedfrom != null)
            addedfrom = new Date($scope.vm.filters.addedfrom.toString().split('/').reverse().join('-'));

        if ($scope.vm.filters.addedto && $scope.vm.filters.addedto != null)
            addedto = new Date($scope.vm.filters.addedto.toString().split('/').reverse().join('-'));

        var searchCondition = {
            'customerid': ($scope.vm.selectedCustomer && $scope.vm.selectedCustomer != null) ? $scope.vm.selectedCustomer.id : 0,
            'addedfrom': addedfrom,
            'addedto': addedto,
            'typeid': -1,
            'status': -1,
            'billlocationid': $scope.vm.filters.billlocationid,
        };

        if (addedfrom == '' || addedto == '') {
            $scope.vm.showToast('Please select date range', 'warning', 1);
            return;
        }

        $scope.vm.apiPost('Searching ...Orders', 'BillinboxService/Search', { 'item': searchCondition }).then(function (response) {
            $scope.vm.Bills = response.data;
            angular.forEach($scope.vm.Bills, function (item) {
                if (item.billimg && item.billimg != null && item.billimg != '') {
                    item.lrimg = item.billimg.split(',')[0];
                }
            });
            $scope.billlimit = 20;
        });
    }

    $scope.billlimit = 20; // initial value for limit
    $scope.loadMoreBills = function (last, inview) {
        if (last && inview) {
            $scope.billlimit += 10
        }
    }

    $scope.vm.DownloadPdf_bill = function (item) {
        $scope.vm.Download('BillService/PrintPDF/0/' + item.billid + '/false', 'BILLPDF-' + item.billno + '.pdf');
    }

    $scope.vm.SearchRef = function (id, field, list) {
        var exist = list.find(function (item) {
            return item.id == id;
        });
        if (exist && exist != null)
            return exist[field];

        return '';
    }

    $scope.vm.SearchOrder = function () {

        var fromdate = '';
        var todate = '';
        if ($scope.vm.filters.fromDate && $scope.vm.filters.fromDate != null)
            fromdate = new Date($scope.vm.filters.fromDate.toString().split('/').reverse().join('-'));

        if ($scope.vm.filters.toDate && $scope.vm.filters.toDate != null)
            todate = new Date($scope.vm.filters.toDate.toString().split('/').reverse().join('-'));

        var searchCondition = {
            'customerid': ($scope.vm.selectedCustomer && $scope.vm.selectedCustomer != null) ? $scope.vm.selectedCustomer.id : 0,
            'fromDate': fromdate,
            'toDate': todate,
            'sono': $scope.vm.filters.sono,
            'deliverylocationid': $scope.vm.filters.deliverylocationid,
        };

        if (fromdate == '' || todate == '') {
            $scope.vm.showToast('Please select date range', 'warning', 1);
            return;
        }
        $scope.vm.apiPost('Searching ...Orders', 'SalesorderService/Search', { 'item': searchCondition }).then(function (response) {
            $scope.vm.Orders = response.data;
            $scope.orderlimit = 20;
            $scope.EditMode = false;
        });
    }

    $scope.orderlimit = 20; // initial value for limit
    $scope.loadMoreOrders = function (last, inview) {
        if (last && inview) {
            $scope.orderlimit += 10
        }
    }

    $scope.EditMode = false;
    $scope.vm.salesorder = null;
    $scope.vm.editOrder = function (item) {
        $scope.vm.apiPost('Searching ...Orders', 'MfgReportService/SearchInGallery', { 'item': { 'salesorderid': item.id } }).then(function (response) {
            $scope.vm.designsdata = response.data;
            $scope.vm.designs = $scope.vm.designsdata.items;
            $scope.vm.availDesigns = $scope.vm.designs.length;
            $scope.vm.groupedDesigns = [];
            $scope.vm.colourgroup = $scope.vm.designsdata.colourgroup;
            $scope.vm.salesorder = $scope.vm.designsdata.salesorder;
            $scope.vm.deliverylocationid = $scope.vm.salesorder.deliverylocationid;
            $scope.vm.companyid = $scope.vm.salesorder.companyid;
            $scope.vm.salesmenid = $scope.vm.salesorder.salesmenid;
            $scope.vm.agentid = $scope.vm.salesorder.agentid;
            $scope.vm.companyid = $scope.vm.salesorder.companyid;
            $scope.vm.companyChanged();

            if ($scope.vm.salesorder.sizegroup &&
                $scope.vm.salesorder.sizegroup != '') {
                var selectedsg = $scope.vm.fullsizegroup.find(function (item) {
                    return item.name == $scope.vm.salesorder.sizegroup;
                });
                if (selectedsg && selectedsg != null) {
                    $scope.vm.filters.sizegroupid = selectedsg.id;
                    $scope.vm.sizegroupchanged();
                }
            }
                 
            $scope.limit = 20

            angular.forEach($scope.vm.designs, function (item) {
                var filtereditems = $scope.vm.salesorder.Salesorderitems.filter(function (sitem) {
                    return sitem.designitemcode == item.designitemcode;
                });
                angular.forEach($scope.vm.designsdata.sizelist, function (sizeitem) {
                    var order = filtereditems.find(function (sitem) { return sitem.sizeid == sizeitem.id; });
                    if (item.order == undefined || item.order == null) item.order = {};
                    item.order[sizeitem.name] = (order && order != null) ? order.oqty : 0;
                });
            });

            var selectedItem = $scope.vm.customers.find(c => c.id === $scope.vm.salesorder.customerid);
            if (selectedItem && selectedItem != null) {
                selectedItem.cityname = $scope.vm.cities.find(c => c.id === selectedItem.cityid)?.name || null;
                selectedItem.statename = $scope.vm.states.find(c => c.id === selectedItem.stateid)?.name || null;
            }
            $scope.vm.selectedCustomer = selectedItem;

            $scope.vm.viewMode = 'Catalog';
            $scope.vm.modifiedorders = $scope.vm.designs;
            $scope.vm.mergeModifiedOrders();
            $scope.vm.recalculateSizeTotals();
            $scope.vm.calculateTotals();
            $scope.EditMode = true;
            if ($scope.vm.perRow == -1) {
                $scope.vm.processGroupedDesign();
            }
        });
    }

    $scope.gdlimit = 10; // initial value for limit
    $scope.loadMoreGroup = function (last, inview) {
        if (last && inview) {
            $scope.gdlimit += 5
        }
    }


    $scope.limit = 20; // initial value for limit
    $scope.loadMore = function (last, inview) {
        if (last && inview) {
            $scope.limit += 10
        }
    }

    $scope.customerlimit = 20;
    $scope.loadMoreCustomer = function (last, inview) {
        if (last && inview) {
            $scope.customerlimit += 10
        }
    }

    $scope.orderlimit = 20; // initial value for limit
    $scope.loadMoreOrder = function (last, inview) {
        if (last && inview) {
            $scope.orderlimit += 10
        }
    }

    $scope.vm.showImageViewer = false;
    $scope.vm.activeImageIndex = 0;

    $scope.vm.openImageViewer = function (index) {
        $scope.vm.activeImageIndex = index;
        $scope.vm.showImageViewer = true;
        
        // Add touch and mouse event listeners when image viewer opens
        $timeout(function() {
            var modal = document.getElementById('imageViewerModal');
            if (modal) {
                // Touch events for mobile devices
                modal.addEventListener('touchstart', $scope.vm.handleTouchStart, {passive: false});
                modal.addEventListener('touchmove', $scope.vm.handleTouchMove, {passive: false});
                modal.addEventListener('touchend', $scope.vm.handleTouchEnd, {passive: false});
                
                // Mouse events for desktop testing
                modal.addEventListener('mousedown', $scope.vm.handleMouseDown);
                modal.addEventListener('mouseup', $scope.vm.handleMouseUp);
            }
        }, 100);
    };

    $scope.vm.closeImageViewer = function () {
        $scope.vm.showImageViewer = false;
        
        // Remove touch and mouse event listeners when image viewer closes
        var modal = document.getElementById('imageViewerModal');
        if (modal) {
            modal.removeEventListener('touchstart', $scope.vm.handleTouchStart);
            modal.removeEventListener('touchmove', $scope.vm.handleTouchMove);
            modal.removeEventListener('touchend', $scope.vm.handleTouchEnd);
            modal.removeEventListener('mousedown', $scope.vm.handleMouseDown);
            modal.removeEventListener('mouseup', $scope.vm.handleMouseUp);
        }
    };

    $scope.vm.nextImage = function () {
        if ($scope.vm.activeImageIndex < $scope.vm.designs.length - 1) {
            $scope.vm.activeImageIndex++;
        }
    };

    $scope.vm.prevImage = function () {
        if ($scope.vm.activeImageIndex > 0) {
            $scope.vm.activeImageIndex--;
        }
    };

    // Touch swipe functionality for image viewer
    $scope.vm.touchStartX = 0;
    $scope.vm.touchStartY = 0;
    $scope.vm.touchEndX = 0;
    $scope.vm.touchEndY = 0;
    $scope.vm.minSwipeDistance = 50; // Minimum distance for a swipe
    $scope.vm.isSwipeActive = false;

    $scope.vm.handleTouchStart = function(event) {
        console.log('Touch start event triggered');
        if (event.touches && event.touches.length > 0) {
            $scope.vm.touchStartX = event.touches[0].clientX;
            $scope.vm.touchStartY = event.touches[0].clientY;
            $scope.vm.isSwipeActive = true;
            console.log('Touch start:', $scope.vm.touchStartX, $scope.vm.touchStartY);
        }
    };

    $scope.vm.handleTouchMove = function(event) {
        if ($scope.vm.isSwipeActive) {
            // Prevent default scrolling behavior during swipe
            event.preventDefault();
        }
    };

    $scope.vm.handleTouchEnd = function(event) {
        console.log('Touch end event triggered');
        if (event.changedTouches && event.changedTouches.length > 0 && $scope.vm.isSwipeActive) {
            $scope.vm.touchEndX = event.changedTouches[0].clientX;
            $scope.vm.touchEndY = event.changedTouches[0].clientY;
            
            var deltaX = $scope.vm.touchEndX - $scope.vm.touchStartX;
            var deltaY = $scope.vm.touchEndY - $scope.vm.touchStartY;
            
            console.log('Touch end - deltaX:', deltaX, 'deltaY:', deltaY);
            
            // Check if it's a horizontal swipe (more horizontal than vertical movement)
            if (Math.abs(deltaX) > Math.abs(deltaY) && Math.abs(deltaX) > $scope.vm.minSwipeDistance) {
                if (deltaX > 0) {
                    // Swipe right - go to previous image
                    console.log('Swipe right - previous image');
                    $scope.vm.prevImage();
                    $scope.$apply(); // Trigger Angular digest cycle
                } else {
                    // Swipe left - go to next image
                    console.log('Swipe left - next image');
                    $scope.vm.nextImage();
                    $scope.$apply(); // Trigger Angular digest cycle
                }
            }
            $scope.vm.isSwipeActive = false;
        }
    };

    // Alternative mouse event handlers for testing on desktop
    $scope.vm.handleMouseDown = function(event) {
        $scope.vm.touchStartX = event.clientX;
        $scope.vm.touchStartY = event.clientY;
        $scope.vm.isSwipeActive = true;
        console.log('Mouse down:', $scope.vm.touchStartX, $scope.vm.touchStartY);
    };

    $scope.vm.handleMouseUp = function(event) {
        if ($scope.vm.isSwipeActive) {
            $scope.vm.touchEndX = event.clientX;
            $scope.vm.touchEndY = event.clientY;
            
            var deltaX = $scope.vm.touchEndX - $scope.vm.touchStartX;
            var deltaY = $scope.vm.touchEndY - $scope.vm.touchStartY;
            
            console.log('Mouse up - deltaX:', deltaX, 'deltaY:', deltaY);
            
            // Check if it's a horizontal swipe (more horizontal than vertical movement)
            if (Math.abs(deltaX) > Math.abs(deltaY) && Math.abs(deltaX) > $scope.vm.minSwipeDistance) {
                if (deltaX > 0) {
                    // Swipe right - go to previous image
                    console.log('Mouse swipe right - previous image');
                    $scope.vm.prevImage();
                    $scope.$apply();
                } else {
                    // Swipe left - go to next image
                    console.log('Mouse swipe left - next image');
                    $scope.vm.nextImage();
                    $scope.$apply();
                }
            }
            $scope.vm.isSwipeActive = false;
        }
    };

    $scope.vm.addFromImageViewer = function () {
        $scope.vm.showImageViewer = false;
        setTimeout(function () {
            // Find card div by imageid
            //var card = document.querySelector('[data-imageid="' + $scope.vm.designs[$scope.vm.activeImageIndex].imgid + '"]');
            var card = document.querySelector('[data-imageid="' + $scope.vm.activeImageIndex + '"]');
            if (card) {
                // Find the first input inside the card
                var input = card.querySelector('input[type="number"], input[type="text"]');
                if (input) {
                    input.focus();
                    input.select(); // also select text
                }
            }
        }, 300);
    };


    $scope.vm.cleanModifiedOrders = function () {
        // Remove items where all sizes are 0
        $scope.vm.modifiedorders = $scope.vm.modifiedorders.filter(function (item) {
            let hasQty = false;
            angular.forEach(item.order, function (qty) {
                if (parseInt(qty) > 0) {
                    hasQty = true;
                }
            });
            return hasQty;
        });
        localStorage.setItem('filters', JSON.stringify($scope.vm.filters));
    };

    $scope.vm.saveAllOrders = function () {
        if ($scope.vm.selectedCustomer == undefined || $scope.vm.selectedCustomer == null ||
            $scope.vm.selectedCustomer.id == undefined || $scope.vm.selectedCustomer.id <= 0) {
            $scope.vm.showToast('Please select the customer', 'warning', 1);
            return;
        }

        if ($scope.vm.deliverylocationid == undefined || $scope.vm.deliverylocationid == null || $scope.vm.deliverylocationid <= 0) {
            $scope.vm.showToast('Please select the location', 'warning', 1);
            return;
        }

        if ($scope.vm.totalQty <= 0) {
            $scope.vm.showToast('Please select minimum one design', 'warning', 1);
            return;
        }

        var filtereddesigns = $scope.vm.searchOrderList();

        var salesorderitems = [];
        filtereddesigns.forEach(function (item) {
            if (item.order && item.order != null) {
                Object.keys(item.order).forEach(function (size) {
                    var qty = item.order[size];
                    var sizeId = $scope.vm.sizeNameIdMap[size];
                    var sizeData = item.stksizes.find(function (ss) {
                        return ss.id === sizeId;
                    });

                    var rate = sizeData.mrp;
                    switch ($scope.vm.pricesource) {
                        case 2:
                            rate = sizeData.rate;
                            break;
                        case 1:
                            rate = sizeData.wsrate;
                            break;
                        case 0:
                            rate = sizeData.salerate;
                            break;
                    }

                    salesorderitems.push({
                        'designitemcode': item.designitemcode,
                        'size': size,
                        'sizeid': sizeId,
                        'uid': sizeData && sizeData != null ? sizeData.uid : '',
                        'price': sizeData && sizeData != null ? sizeData.rate : 0,
                        'mrp': sizeData && sizeData != null ? sizeData.mrp : 0,
                        'oqty': parseInt(qty) || 0,
                    });
                });
            }
        });

        var selectedsg = $scope.vm.fullsizegroup.find(function (item) {
            return item.id == $scope.vm.filters.sizegroupid;
        });

        if ($scope.vm.salesorder == undefined || $scope.vm.salesorder == null)
            $scope.vm.salesorder = {};

        $scope.vm.salesorder.deliverylocationid = $scope.vm.deliverylocationid;
        $scope.vm.salesorder.companyid = $scope.vm.companyid;
        $scope.vm.salesorder.customerid = $scope.vm.selectedCustomer.id;
        $scope.vm.salesorder.sizegroup = selectedsg && selectedsg != null ? selectedsg.name:'';

        var salesorder = {
            'companyid': $scope.vm.companyid,
            'customerid': $scope.vm.selectedCustomer.id,
            'deliverylocationid': $scope.vm.deliverylocationid,
            'orderitems': salesorderitems,
            'salesmenid': $scope.vm.salesmenid,
            'soboxid': $scope.vm.soboxid,
            'agentid': $scope.vm.agentid,
            'remarks': $scope.vm.remarks,
            'sizegroup': selectedsg && selectedsg != null ? selectedsg.name : '',
            'currentorder': $scope.vm.salesorder
        }

        $scope.vm.apiPost('Saving ...Order', 'SalesorderService/SaveCatalog', { 'item': salesorder }).then(function (response) {
            if (response.data != null && response.data.id > 0) {
                $scope.vm.editOrder(response.data);
            }
        });
    };


    $scope.vm.searchOrderList = function () {
        return $scope.vm.modifiedorders.filter(item => item.order && Object.values(item.order).some(qty => parseInt(qty) > 0));
    }

    $scope.vm.downloadPdf = function (item) {
        $scope.vm.Download('SalesorderService/PrintPDF/0/' + item.id, 'SOPDF-' + item.sono + '.pdf');
    }

    $scope.vm.Download = function (url, filename) {
        var config = {
            responseType: 'blob', headers: {
                'Authorization': 'Bearer ' + $scope.vm.authData.token
            }
        };
        $http.get(url, config).then(function (response) {
            var blob = response.data;
            if (filename.indexOf('.') < 0) {
                if (blob.type == 'application/zip')
                    filename += '.zip';
                if (blob.type == 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet')
                    filename += '.xlsx';
            }
            if (window.navigator && window.navigator.msSaveOrOpenBlob) {
                window.navigator.msSaveOrOpenBlob(blob, filename);
            } else {
                var e = document.createEvent('MouseEvents'),
                    a = document.createElement('a');
                a.download = filename;
                a.href = window.URL.createObjectURL(blob);
                a.dataset.downloadurl = filename;
                e.initEvent('click', true, false, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);
                a.dispatchEvent(e);
                window.URL.revokeObjectURL(a.href);
                if (callback && callback != null)
                    callback();
            }
        });
    }

    $scope.vm.previewAllOrders = function () {
        var filtereddesigns = $scope.vm.searchOrderList();
        if (filtereddesigns.length == 0) {
            $scope.vm.showToast('Please select minimum one design', 'warning', 1);
            return;
        }
        $scope.vm.designs = filtereddesigns;
        $scope.vm.availDesigns = $scope.vm.designs.length;
    }

    $scope.vm.loadModifiedOrders = function () {
        var saved = localStorage.getItem('modifiedorders');
        if (saved) {
            try {
                $scope.vm.modifiedorders = JSON.parse(saved);
                if ($scope.vm.modifiedorders && $scope.vm.modifiedorders.length > 0) {
                    $scope.vm.designs = JSON.parse(saved)
                    $scope.vm.mergeModifiedOrders();
                    $scope.vm.recalculateSizeTotals();
                    $scope.vm.calculateTotals();
                    $scope.vm.availDesigns = $scope.vm.designs.length;
                }

            } catch (e) {
                console.error('Failed to parse saved modified orders', e);
                $scope.vm.modifiedorders = [];
            }
        } else {
            $scope.vm.modifiedorders = [];
        }

        var saved = localStorage.getItem('filters');
        if (saved) {
            try {
                $scope.vm.filters = JSON.parse(saved);
                $scope.vm.sizegroupchanged();
            } catch (e) {
                //console.error('Failed to parse saved modified orders', e);
            }
        }

        var saved = localStorage.getItem('selectedCustomer');
        if (saved) {
            try {
                $scope.vm.selectedCustomer = JSON.parse(saved);
            } catch (e) {
                //console.error('Failed to parse saved modified orders', e);
            }
        }
    };

    $scope.vm.mergeModifiedOrders = function () {
        var mergedlist = [];

        $scope.vm.modifiedorders = $scope.vm.modifiedorders.filter(function (modifiedItem) {
            var match = $scope.vm.designs.find(function (newItem) {
                return newItem.designitemcode === modifiedItem.designitemcode;
            });

            if (match) {
                match.order = angular.copy(modifiedItem.order);
                mergedlist.push(match);
                return false; // Remove matched
            }
            return true; // Keep unmatched
        });

        // Now correctly add merged back
        Array.prototype.push.apply($scope.vm.modifiedorders, mergedlist);
    };

    $scope.vm.createNewSO = function () {
        angular.forEach($scope.vm.designs, function (item) {
            item._oldOrder = {};
            if (item.order == undefined || item.order == null)
                item.order = {};
            angular.forEach($scope.vm.sizes, function (size) {
                item.order[size] = '';
                item._oldOrder[size] = '';
            });
        });
        $scope.vm.modifiedorders = [];
        $scope.vm.salesorder = null;
        $scope.vm.sizeTotals = [];
        $scope.vm.totalDesigns = 0;
        $scope.vm.totalQty = 0;
        $scope.vm.sizeTotals = [];
    }

    $scope.vm.getOrderQty = function (item) {
        if (item == undefined || item == null) return '';
        let itemQty = 0;
        if (item.order && item.order != null) {
            angular.forEach($scope.vm.sizes, function (size) {
                if (item.order[size] && item.order[size] != null) {
                    const count = parseInt(item.order[size]) || 0;
                    itemQty += count;
                }
            });
        }
        return itemQty;
    };

    $scope.vm.getStockInfo = function (item, sizeName) {
        var sizeId = $scope.vm.sizeNameIdMap[sizeName];

        if (!sizeId) {
            return { stock: 0, so: 0, available: 0, wip: 0 };
        }

        var sizeData = item.stksizes.find(function (ss) {
            return ss.id === sizeId;
        });

        if (!sizeData) {
            return { stock: 0, so: 0, available: 0, wip: 0 };
        }

        var stock = parseInt(sizeData.stk) || 0;
        var so = parseInt(sizeData.so) || 0;
        var wip = parseInt(sizeData.wip) || 0;
        var available = (stock+wip) - so;

        return { stock: stock, so: so, wip: wip, available: available };
    };

    $scope.vm.getGroupStockInfo = function (grpitem, item, sizeName) {
        if (!item.showmimgid)
            return $scope.vm.getStockInfo(item, sizeName);

        var sizeId = $scope.vm.sizeNameIdMap[sizeName];

        if (!sizeId) {
            return { stock: 0, so: 0, available: 0, wip: 0, order: 0 };
        }
        var grptotal = { stock: 0, so: 0, available: 0, wip: 0,order:0 };

        angular.forEach(grpitem.designs, function (designitem) {
            var itmtotal = $scope.vm.getStockInfo(designitem, sizeName);
            var orderqty = 0;
            if (designitem.order && designitem.order[sizeName]) {
                orderqty = parseInt(designitem.order[sizeName]);
                if (isNaN(orderqty)) orderqty = 0;
            }
            grptotal.order += orderqty;
            grptotal.stock += itmtotal.stock;
            grptotal.so += itmtotal.so;
            grptotal.available += itmtotal.available;
            grptotal.wip += itmtotal.wip;
        });

        return grptotal;
    }

    $scope.vm.sizeTotals = [];
    $scope.vm.modifiedorders = [];

    $scope.vm.recalculateSizeTotals = function () {
        $scope.vm.sizeTotals = {};  // Reset

        $scope.vm.modifiedorders.forEach(function (item) {
            angular.forEach(item.order, function (qty, size) {
                if (parseInt(qty) > 0) {
                    $scope.vm.sizeTotals[size] = ($scope.vm.sizeTotals[size] || 0) + parseInt(qty);
                }
            });
        });
    };

    $scope.vm.updateSizeSummary = function (item, size) {
        if (!item._oldOrder) {
            item._oldOrder = {};
        }
        if (item._oldOrder[size] === undefined) {
            item._oldOrder[size] = 0;
        }

        // Update totals
        $scope.vm.sizeTotals[size] = ($scope.vm.sizeTotals[size] || 0) - (parseInt(item._oldOrder[size]) || 0);
        $scope.vm.sizeTotals[size] = ($scope.vm.sizeTotals[size] || 0) + (parseInt(item.order[size]) || 0);

        // Update old value
        item._oldOrder[size] = parseInt(item.order[size]) || 0;

        // Add to modified if not already
        if ($scope.vm.modifiedorders.indexOf(item) === -1) {
            $scope.vm.modifiedorders.push(item);
        }
        // Final recalculate everything
        $scope.vm.calculateTotals();
    };


    $scope.vm.calculateTotals = function () {
        let designs = 0;
        let qty = 0;
        let sizes = {};

        angular.forEach($scope.vm.modifiedorders, function (item) {
            let itemQty = 0;
            if (item.order && item.order != null) {
                angular.forEach($scope.vm.sizes, function (size) {
                    if (item.order[size] && item.order[size] != null) {
                        const count = parseInt(item.order[size]) || 0;
                        itemQty += count;
                        sizes[size] = (sizes[size] || 0) + count;
                    }
                });
                if (itemQty > 0) {
                    designs++;
                    qty += itemQty;
                }
            }
        });

        $scope.vm.totalDesigns = designs;
        $scope.vm.totalQty = qty;
        $scope.vm.sizeTotals = sizes;
        $scope.vm.ConfirmWidow.SetEnable(true);
    };


    $scope.vm.cleanZeroOrInvalid = function (item, size) {
        if (item.order == undefined)
            return;
        if (item.order[size] == undefined)
            item.order[size] = 0;

        const value = item.order[size];
        if (value === 0 || value === '0' || isNaN(parseInt(value))) {
            item.order[size] = '';
        }
    };
    $scope.vm.showProgress = false;
    $scope.vm.progressMessage = '';

    $scope.vm.sizegroupchanged = function () {
        if ($scope.vm.filters.sizegroupid && $scope.vm.filters.sizegroupid > 0) {
            var selectedsg = $scope.vm.fullsizegroup.find(function (item) {
                return item.id == $scope.vm.filters.sizegroupid;
            });
            if (selectedsg && selectedsg != null &&
                selectedsg.possizelist && selectedsg.possizelist != null) {
                $scope.vm.sizes = selectedsg.possizelist.split(',');
            }
        }
        localStorage.setItem('sizes', JSON.stringify($scope.vm.sizes));
    }



    // Toast Variables
    $scope.vm.toastMessage = '';
    $scope.vm.toastType = 'warning';  // default
    $scope.vm.toastClosing = false;

    // Show Toast
    $scope.vm.showToast = function (message, type, durationInSeconds) {
        $scope.vm.toastMessage = message;
        $scope.vm.toastType = type || 'warning';  // type: success, error, warning, info
        $scope.vm.toastClosing = false;

        if ($scope.vm.toastTimeout) {
            clearTimeout($scope.vm.toastTimeout);
        }

        $scope.vm.toastTimeout = setTimeout(function () {
            $scope.$apply(function () {
                $scope.vm.toastClosing = true;
            });
            setTimeout(function () {
                $scope.$apply(function () {
                    $scope.vm.toastMessage = '';
                });
            }, 300); // smooth fade
        }, durationInSeconds * 1000);
    };

    // Manual Close
    $scope.vm.closeToast = function () {
        if ($scope.vm.toastTimeout) {
            clearTimeout($scope.vm.toastTimeout);
        }
        $scope.vm.toastClosing = true;
        setTimeout(function () {
            $scope.$apply(function () {
                $scope.vm.toastMessage = '';
            });
        }, 300);
    };

    $scope.vm.viewMode = 'Catalog';
    $scope.vm.toggleCatalogOrder = function () {
        if ($scope.vm.viewMode === 'Catalog') {
            $scope.vm.viewMode = 'Order';
        } else {
            $scope.vm.viewMode = 'Catalog';
        }

        $scope.vm.onViewModeChange();
    };

    // Optional: Event to handle view change
    $scope.vm.onViewModeChange = function () {
        console.log("View Mode Changed to:", $scope.vm.viewMode);
        // You can do something like:
        // if (vm.viewMode == 'Order') { filterOnlyOrderedDesigns(); }
    };

    $scope.vm.setMode = function (mode) {
        $scope.vm.viewMode = mode;
        $scope.vm.showMenu = false;
        $scope.vm.showdMenu = false;
        if (mode != 'Catalog') {
            $scope.vm.showFilters = true;
        }
        $scope.vm.onViewModeChange();
    }

    $scope.vm.hasQty = function (order) {
        return Object.values(order || {}).some(qty => parseInt(qty) > 0);
    };

    $scope.vm.getTotalQty = function (size) {
        let total = 0;
        ($scope.vm.designs || []).forEach(item => {
            const val = parseInt(item?.order?.[size]);
            if (!isNaN(val)) total += val;
        });
        return total;
    };

    $scope.vm.companyChanged = function () {
        var company = $scope.vm.company.find(function (item) {
            return item.id == $scope.vm.companyid;
        });
        if (company && company != null)
            $scope.vm.selectedCompany = company;
        else
            $scope.vm.selectedCompany = null;
    }

    $scope.vm.printPreview = function () {
        const printContents = document.querySelector('.PrintPreviewContent').innerHTML;
        const win = window.open('', '', 'width=900,height=700');
        win.document.write('<html><head><title>Order Preview</title>');
        win.document.write('<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/tailwindcss@2.2.19/dist/tailwind.min.css">');
        win.document.write(`<style>
  thead { display: table-header-group; }
  tfoot { display: table-footer-group; }
  .break-inside-avoid { break-inside: avoid; }
  .break-inside-avoid-page { page-break-inside: avoid; }
  .print-header {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    padding: 1rem;
    background: white;
    z-index: 1000;
  }
  body { margin-top: 180px; }
</style>`)
        win.document.write('</head><body class="p-4">');
        win.document.write(printContents);
        win.document.write('</body></html>');
        win.document.close();
        win.focus();
        win.print();
    };

    $scope.vm.generatePdf = function () {
        const element = document.querySelector('.PrintPreviewContent');
        var vm = $scope.vm;
        html2pdf().set({
            margin: 0.2,
            filename: `Order-${vm.salesorder.sono}.pdf`,
            image: { type: 'jpeg', quality: 0.98 },
            html2canvas: { scale: 2, useCORS: true },
            jsPDF: {
                unit: 'in',
                format: [8.5, 11], // width, height in inches
                orientation: 'portrait'
            },
            pagebreak: {
                mode: ['css', 'legacy'],
                before: ['.break-before-page'],
                avoid: ['.break-inside-avoid']
            }
        }).from(element).outputPdf('blob').then(function (blob) {
            const file = new File([blob], `Order-${vm.salesorder.sono}.pdf`, { type: 'application/pdf' });

            // Check if mobile + Web Share API with file support is available
            //if (navigator.canShare && navigator.canShare({ files: [file] })) {
            //    navigator.share({
            //        title: `Order ${vm.salesorder.sono}`,
            //        text: 'Sharing sales order PDF',
            //        files: [file]
            //    }).catch(err => console.error('Share failed', err));
            //}

            //else {
                // Desktop fallback: create download & WhatsApp link
                const url = URL.createObjectURL(file);

                const a = document.createElement('a');
                a.href = url;
                a.download = file.name;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(url);

                // Optional: suggest user to share manually via WhatsApp Web
                const message = encodeURIComponent(`Order ${vm.salesorder.sono} is ready. Please find the PDF attached.`);
                const whatsappURL = `https://web.whatsapp.com/send?text=${message}`;
                window.open(whatsappURL, '_blank');
            //}
        });
    };

    angular.element($window).bind('resize', function () {
        $scope.$apply(function () {
            $scope.vm.isDesktop = $scope.isDesktopDevice();
            if ($scope.vm.isDesktop) {
                //$scope.vm.showFilters = true;
            }
        });
    });

    setTimeout(function () {
        flatpickr("#fromDatePicker", {
            dateFormat: "d/m/Y",
            defaultDate: new Date(),
            onChange: function (selectedDates, dateStr, instance) {
                $scope.$apply(function () {
                    $scope.vm.filters.fromDate = dateStr;
                });
            }
        });
        flatpickr("#toDatePicker", {
            dateFormat: "d/m/Y",
            defaultDate: new Date(),
            onChange: function (selectedDates, dateStr, instance) {
                $scope.$apply(function () {
                    $scope.vm.filters.toDate = dateStr;
                });
            }
        });

        const menuIds = ['menuContainer1', 'menuContainer2', 'menuContainer3', 'menuContainer4'];
        const menuElements = menuIds.map(id => document.getElementById(id));

        document.addEventListener('click', function (event) {
            $timeout(function () {
                if (!$scope.vm.showMenu && !$scope.vm.showdMenu) return;

                if (!menuElements.some(el => el && el.contains(event.target))) {
                    $scope.vm.showMenu = false;
                    $scope.vm.showdMenu = false;
                    $scope.$applyAsync();
                }
            }, 50);
        });

        $timeout(function () {
            $scope.vm.RestoreLayout();


            $scope.vm.showMoveTop = false;   // show “Move Top” when scrolled past top
            $scope.vm.canRestore = false;   // show “Back” after user clicked Move Top
            $scope.vm._savedY = null;    // saved scroll Y

            // Detect when user moved away from the first page/top
            var onScroll = function () {
                // If your list is in the main window scroll:
                var scrolled = $window.pageYOffset || document.documentElement.scrollTop || 0;
                $scope.vm.showMoveTop = scrolled > 200; // threshold
                // Note: $applyAsync avoids digest errors while keeping it snappy.
                $scope.$applyAsync();
            };

            angular.element($window).on('scroll', onScroll);
            $scope.$on('$destroy', function () {
                angular.element($window).off('scroll', onScroll);
            });

            // Save current position and go to the top sentinel
            $scope.vm.savePosAndGoTop = function () {
                $scope.vm._savedY = $window.pageYOffset || document.documentElement.scrollTop || 0;
                $scope.vm.canRestore = true;
                // Smooth scroll to the sentinel
                var topEl = document.getElementById('designListTop');
                //if (topEl) {
                //    topEl.scrollIntoView({ behavior: 'smooth', block: 'start' });
                //} else {
                    $window.scrollTo({ top: 0, behavior: 'smooth' });
                //}
            };

            // Restore to saved position
            $scope.vm.restorePosition = function () {
                if ($scope.vm._savedY != null) {
                    // Use timeout so if content just changed (infinite load), layout can settle
                    $timeout(function () {
                        $window.scrollTo({ top: $scope.vm._savedY, behavior: 'smooth' });
                        // Optionally hide Back after restore:
                        // vm.canRestore = false;
                    }, 0);
                }
            };


        }, 100);

    }, 10);

    $scope.vm.loadModifiedOrders();
    var today = new Date(), day = ('0' + today.getDate()).slice(-2), month = ('0' + (today.getMonth() + 1)).slice(-2), year = today.getFullYear();
    $scope.vm.filters.fromDate = today;
    $scope.vm.filters.toDate = today;
});

app.directive('autoNextOnNumber', function () {
    return {
        restrict: 'A',
        link: function (scope, element) {
            element.on('keydown', function (e) {
                if (e.key >= '0' && e.key <= '9') {
                    e.preventDefault();

                    const inputs = document.querySelectorAll('.size-input');
                    const index = Array.prototype.indexOf.call(inputs, element[0]);

                    if (index < inputs.length - 1) {
                        inputs[index + 1].focus();
                    }
                }
            });
        }
    };
});
