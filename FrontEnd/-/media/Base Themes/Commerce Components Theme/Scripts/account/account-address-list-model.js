// -----------------------------------------------------------------------
// Copyright 2016 Sitecore Corporation A/S
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
// except in compliance with the License. You may obtain a copy of the License at
//       http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
// either express or implied. See the License for the specific language governing permissions
// and limitations under the License.
// -------------------------------------------------------------------------------------------

function AddressListViewModel(data, addressPageUrl) {
  var AddressViewModel = function (address, addressPageUrl) {
    var self = this;

    if (address && addressPageUrl) {
      if (address.ExternalId) {
        addressPageUrl = addressPageUrl + "?id=" + address.ExternalId;
      }

      address.DetailsUrl = addressPageUrl;
    }

    self.externalId = address ? ko.observable(address.ExternalId) : ko.observable();
    self.partyId = address ? ko.observable(address.ExternalId) : ko.observable();
    self.isPrimary = address ? ko.observable(address.IsPrimary) : ko.observable();
    self.fullAddress = address ? ko.observable(address.FullAddress) : ko.observable();
    self.detailsUrl = address ? ko.observable(address.DetailsUrl) : ko.observable();
    self.address1 = address ? ko.observable(address.Address1) : ko.observable();
    self.address2 = address ? ko.observable(address.Address2) : ko.observable();
    self.city = address ? ko.observable(address.City) : ko.observable();
    self.country = address ? ko.observable(address.Country) : ko.observable();
    self.state = address ? ko.observable(address.State) : ko.observable();
    self.name = address ? ko.observable(address.Name) : ko.observable();
  };

  var self = this;

  self.addresses = ko.observableArray();
  if (data && data.Addresses) {
    $.each(data.Addresses, function () {
      self.addresses.push(new AddressViewModel(this, addressPageUrl));
    });
  }

  self.isNotEmpty = ko.observable(self.addresses().length !== 0);
  self.isEmpty = ko.observable(self.addresses().length === 0);
  self.showLoader = ko.observable(true);
}