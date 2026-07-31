using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils;

public class ListView : BaseView
{
	#region Fields

	#region Serialize Fields
	[SerializeField] private ViewSelector _viewSelector;
	[Space]
	[SerializeField] private PoiList _scrollView;
	#endregion Serialize Fields

	#region Public Variables
	#endregion Public Variables

	#region Private m_Variables
	private List<Wezit.Poi> m_pois = null;
	#endregion Private m_Variables
	#endregion Fields

	#region Properties
	#endregion Properties

	#region Methods
	#region MonoBehavior
	#endregion MonoBehavior

	#region Public
	public override void PrepareHideView()
	{
		base.PrepareHideView();
		MapUtils.KeepRotating = false;
	}
	#endregion Public

	#region Private
	protected override void InitViewContentByLang(Language language)
	{
		base.InitViewContentByLang(language);

		PlayerManager.CurrentState.IsFromOnMapButton = false;
		_viewSelector.Init();
		_viewSelector.gameObject.SetActive(!PlayerManager.CurrentState.IsAudioDescription);

        MapUtils.StartLocationService(this);

		m_pois = PlayerManager.CurrentState.CurrentTour.children;

		TourProgressionData tourProgressionData = PlayerManager.Player.GetCurrentTourProgression();

		_scrollView.Inflate(m_pois, tourProgressionData, this);
	}

	protected override void ResetViewContent()
	{
		base.ResetViewContent();

		_scrollView.gameObject.SetActive(true);
		_scrollView.ResetContent();
	}

	protected override void AddListeners()
	{
		base.AddListeners();
	}

	protected override void RemoveListeners()
	{
		base.RemoveListeners();
	}
	#endregion Private
	#endregion Methods
}