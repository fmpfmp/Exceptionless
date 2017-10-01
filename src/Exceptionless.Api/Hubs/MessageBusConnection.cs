﻿using System;
using System.Threading.Tasks;
using Exceptionless.Core.Extensions;
using Exceptionless.Core.Utility;
using Microsoft.AspNet.SignalR;
using Microsoft.Extensions.Logging;

namespace Exceptionless.Api.Hubs {
    public class MessageBusConnection : PersistentConnection {
        private readonly IConnectionMapping _connectionMapping;
        private readonly ILogger _logger;

        public MessageBusConnection(IConnectionMapping connectionMapping, ILogger<MessageBusConnection> logger) {
            _connectionMapping = connectionMapping;
            _logger = logger;
        }

        protected override async Task OnConnected(IRequest request, string connectionId) {
            try {
                foreach (var organizationId in request.User.GetOrganizationIds())
                    await _connectionMapping.GroupAddAsync(organizationId, connectionId);

                await _connectionMapping.UserIdAddAsync(request.User.GetUserId(), connectionId);
            } catch (Exception ex) {
                _logger.LogError(ex, "OnReconnected Error: {Message}", ex.Message);
                throw;
            }
        }

        protected override async Task OnDisconnected(IRequest request, string connectionId, bool stopCalled) {
            try {
                foreach (var organizationId in request.User.GetOrganizationIds())
                    await _connectionMapping.GroupRemoveAsync(organizationId, connectionId);

                await _connectionMapping.UserIdRemoveAsync(request.User.GetUserId(), connectionId);
            } catch (Exception ex) {
                _logger.LogError(ex, "OnDisconnected Error: {Message}", ex.Message);
                throw;
            }
        }

        protected override Task OnReconnected(IRequest request, string connectionId) {
            return OnConnected(request, connectionId);
        }

        protected override bool AuthorizeRequest(IRequest request) {
            return request.User.Identity.IsAuthenticated;
        }
    }
}