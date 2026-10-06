using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Common.Api;
using MediaBrowser.Controller.Devices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.ClientQuality.Api;

/// <summary>
/// A device the server has seen, as shown on the config page.
/// </summary>
/// <param name="Id">The device id.</param>
/// <param name="Name">The device's custom name if set, otherwise its reported name.</param>
/// <param name="AppName">The client app name.</param>
/// <param name="AppVersion">The client app version.</param>
/// <param name="LastUserName">The last user to sign in on the device.</param>
/// <param name="DateLastActivity">When the device was last active.</param>
public sealed record KnownDevice(
    string? Id,
    string? Name,
    string? AppName,
    string? AppVersion,
    string? LastUserName,
    DateTime? DateLastActivity);

/// <summary>
/// Admin-only endpoints backing the config page.
/// </summary>
[ApiController]
[Authorize(Policy = Policies.RequiresElevation)]
[Route("ClientQuality")]
public class ClientQualityController : ControllerBase
{
    private readonly IDeviceManager _deviceManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientQualityController"/> class.
    /// </summary>
    /// <param name="deviceManager">Instance of the <see cref="IDeviceManager"/> interface.</param>
    public ClientQualityController(IDeviceManager deviceManager)
    {
        _deviceManager = deviceManager;
    }

    /// <summary>
    /// Lists every device known to the server, across all users.
    /// </summary>
    /// <response code="200">Devices returned.</response>
    /// <returns>The devices, most recently active first.</returns>
    [HttpGet("Devices")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<KnownDevice>> GetDevices()
    {
        // Core's GET /Devices filters to the calling admin's own devices; passing no user returns all.
        // Map to a slim record so access tokens are never sent to the page.
        return Ok(_deviceManager.GetDevicesForUser(null).Items
            .Select(d => new KnownDevice(
                d.Id,
                string.IsNullOrWhiteSpace(d.CustomName) ? d.Name : d.CustomName,
                d.AppName,
                d.AppVersion,
                d.LastUserName,
                d.DateLastActivity)));
    }
}
